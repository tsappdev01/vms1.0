using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using AE.EmiratesId.IdCard;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Reads an Emirates ID through the ICP toolkit, in-process.
///
/// This works because the application runs ON the reception machine, so its server-side
/// code is on the machine the reader is attached to. No bridge process and no browser
/// interop are involved.
///
/// The toolkit is native and single-threaded per context, so access is serialised.
/// </summary>
public sealed class CardReaderService(IConfiguration configuration, ILogger<CardReaderService> logger)
    : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Toolkit? _toolkit;
    private string? _initialisationError;

    public async Task<ReaderState> GetStateAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!TryInitialise())
            {
                return new ReaderState { Available = false, Detail = _initialisationError ?? "The toolkit is not initialised." };
            }

            string? version = null;
            string? licence = null;
            try { version = _toolkit!.GetToolkitVersion(); } catch { /* reported as unavailable below */ }
            try { licence = _toolkit!.GetLicenseExpiryDate(); } catch { /* ditto */ }

            try
            {
                var reader = _toolkit!.GetReaderWithEmiratesId();
                return new ReaderState
                {
                    Available = true,
                    ReaderName = reader.Name,
                    Detail = "Card detected. Ready to read.",
                    ToolkitVersion = version,
                    LicenceExpiry = licence,
                };
            }
            catch (Exception ex)
            {
                // No card, or no reader. Both are normal states at an idle desk.
                return new ReaderState
                {
                    Available = false,
                    Detail = ex.Message,
                    ToolkitVersion = version,
                    LicenceExpiry = licence,
                };
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<CardData> ReadAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!TryInitialise())
            {
                throw new InvalidOperationException(_initialisationError ?? "The toolkit is not initialised.");
            }

            var reader = _toolkit!.GetReaderWithEmiratesId();
            reader.Connect();

            try
            {
                var requestId = NewRequestId();

                // Non-modifiable data, photograph, the card's signature image and the
                // address. Modifiable data is not read: occupation, sponsor and passport
                // details are not what visitor management is for.
                var data = reader.ReadPublicData(requestId, true, false, true, true, true);

                var warning = ValidateResponse(requestId, data.XmlString);
                var nm = data.NonModifiablePublicData;
                var home = data.HomeAddress;

                return new CardData
                {
                    IdNumber = data.IdNumber,
                    CardNumber = data.CardNumber,
                    Photo = data.CardHolderPhoto?.ToArray(),
                    CardSignature = data.HolderSignatureImage?.ToArray(),

                    IdType = nm?.IdType,
                    IssueDate = nm?.IssueDate,
                    ExpiryDate = nm?.ExpiryDate,
                    FullNameEnglish = CleanName(nm?.FullNameEnglish),
                    FullNameRaw = nm?.FullNameEnglish,
                    FullNameArabic = CleanName(nm?.FullNameArabic),
                    TitleEnglish = nm?.TitleEnglish,
                    Gender = nm?.Gender,
                    DateOfBirth = nm?.DateOfBirth,
                    NationalityEnglish = nm?.NationalityEnglish,
                    NationalityCode = nm?.NationalityCode,
                    PlaceOfBirthEnglish = nm?.PlaceOfBirthEnglish,

                    AddressEmirate = home?.EmirateEnglish,
                    AddressCity = home?.CityEnglish,
                    AddressArea = home?.AreaEnglish,
                    AddressStreet = home?.StreetEnglish,
                    AddressBuilding = home?.BuildingNameEnglish,
                    AddressPoBox = home?.PoBox,
                    AddressPhone = home?.LandPhoneNumber,
                    AddressMobile = home?.MobilePhoneNumber,
                    AddressEmail = home?.Email,

                    SignatureWarning = warning,
                };
            }
            finally
            {
                try { reader.Disconnect(); } catch { /* the card may already be out */ }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Initialises once and remembers the failure, so a misconfigured desk reports the
    /// same clear reason on every attempt rather than retrying a native init in a loop.
    /// </summary>
    private bool TryInitialise()
    {
        if (_toolkit is not null) return true;
        if (_initialisationError is not null) return false;

        try
        {
            var configDirectory = configuration["Toolkit:ConfigDirectory"];

            if (string.IsNullOrWhiteSpace(configDirectory) || !Directory.Exists(configDirectory))
            {
                _initialisationError =
                    "Toolkit:ConfigDirectory is not set to an existing folder. Point it at the " +
                    "ICP config directory - the one containing config_li and config_ag.";
                return false;
            }

            var logDirectory = configuration["Toolkit:LogDirectory"]
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EIDAToolkit", "logs");
            Directory.CreateDirectory(logDirectory);

            var config =
                $"{{\"config_directory\":\"{configDirectory.Replace("\\", "\\\\")}\"," +
                $"\"log_directory\":\"{logDirectory.Replace("\\", "\\\\")}\"," +
                "\"application_type\":\"APP_INPROC\"," +
                "\"read_publicdata_offline\":true}";

            _toolkit = new Toolkit(true, config);
            logger.LogInformation("Toolkit initialised from {ConfigDirectory}", configDirectory);
            return true;
        }
        catch (Exception ex)
        {
            _initialisationError = ex.Message;
            logger.LogError(ex, "Toolkit initialisation failed");
            return false;
        }
    }

    /// <summary>
    /// The chip stores names as comma-delimited segments, most of them empty:
    /// "NAYYAR JAWAID,,,,,ALI KHAN," is one person, not seven fields.
    /// </summary>
    private static string CleanName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var joined = string.Join(' ', value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.Length > 0));

        return joined.Length > 0 ? joined : value.Trim();
    }

    /// <summary>40 cryptographically random bytes, as the vendor sample uses.</summary>
    private static string NewRequestId() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(40));

    /// <summary>
    /// Checks the toolkit's signed XML response against the request that produced it.
    ///
    /// A mismatched request id means the response does not belong to this request, so it
    /// is rejected. A failed signature is returned as a warning rather than thrown:
    /// CheckSignature validates against the key inside the response, so alone it shows
    /// internal consistency rather than provenance. Pinning it to the licence's server
    /// certificate is the stronger control and needs ServerTLSCert, which the current
    /// licence does not carry.
    /// </summary>
    private static string? ValidateResponse(string requestId, string? xml)
    {
        if (string.IsNullOrEmpty(xml)) return null;

        if (!RequestIdMatches(requestId, xml))
        {
            throw new InvalidOperationException(
                "Request ID verification failed - the response may have been tampered with.");
        }

        return VerifySignature(xml) ? null : "The response signature could not be verified.";
    }

    private static bool RequestIdMatches(string requestId, string xml)
    {
        try
        {
            // Parsed before it is trusted, so it must not be able to fetch anything.
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using var reader = XmlReader.Create(new StringReader(xml), settings);
            return reader.ReadToFollowing("RequestID")
                && string.Equals(requestId, reader.ReadElementContentAsString(), StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static bool VerifySignature(string xml)
    {
        try
        {
            var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = false };
            document.LoadXml(xml);

            var nodes = document.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            if (nodes.Count == 0) return false;

            var signedXml = new SignedXml(document);
            signedXml.LoadXml((XmlElement)nodes[0]!);
            return signedXml.CheckSignature();
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        try { _toolkit?.Cleanup(); } catch { /* shutting down anyway */ }
        _gate.Dispose();
    }
}
