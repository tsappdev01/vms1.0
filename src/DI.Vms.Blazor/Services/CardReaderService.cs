using System.Globalization;
using System.Runtime.InteropServices;
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
            string? licenceRaw = null;
            try { version = _toolkit!.GetToolkitVersion(); } catch { /* reported as unavailable below */ }
            try { licenceRaw = _toolkit!.GetLicenseExpiryDate(); } catch { /* ditto */ }

            var (licence, licenceDays) = ReadLicence(licenceRaw);

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
                    LicenceDaysRemaining = licenceDays,
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
                    LicenceDaysRemaining = licenceDays,
                };
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Reads the card, reporting each phase as it starts.
    ///
    /// The toolkit's calls are synchronous native ones, so the phases exist as much for
    /// the interface as for the log: without a real await between them the circuit never
    /// gets a turn to paint, and a read that takes a couple of seconds looks like a
    /// button that did nothing. See <see cref="PhaseAsync"/>.
    /// </summary>
    public async Task<CardData> ReadAsync(IProgress<string>? progress = null)
    {
        await _gate.WaitAsync();
        try
        {
            await PhaseAsync(progress, "Starting the toolkit…");

            if (!TryInitialise())
            {
                throw new InvalidOperationException(_initialisationError ?? "The toolkit is not initialised.");
            }

            await PhaseAsync(progress, "Connecting to the card…");

            var reader = _toolkit!.GetReaderWithEmiratesId();
            reader.Connect();

            try
            {
                var requestId = NewRequestId();

                await PhaseAsync(progress, "Reading the chip…");

                // Non-modifiable data, photograph, the card's signature image and the
                // address. Modifiable data is not read: occupation, sponsor and passport
                // details are not what visitor management is for.
                var data = reader.ReadPublicData(requestId, true, false, true, true, true);

                /* Repaired before validation as well as before display: the signed XML
                   comes through the same mangling, and a digest taken over mangled text
                   never matches the one the card signed. */
                await PhaseAsync(progress, "Checking the response…");

                var warning = ValidateResponse(requestId, CardText.Fix(data.XmlString));
                var nm = data.NonModifiablePublicData;
                var home = data.HomeAddress;

                return new CardData
                {
                    IdNumber = data.IdNumber,
                    CardNumber = data.CardNumber,
                    Photo = data.CardHolderPhoto?.ToArray(),
                    CardSignature = data.HolderSignatureImage?.ToArray(),

                    IdType = CardText.Fix(nm?.IdType),
                    IssueDate = nm?.IssueDate,
                    ExpiryDate = nm?.ExpiryDate,
                    FullNameEnglish = CleanName(nm?.FullNameEnglish),
                    FullNameRaw = CardText.Fix(nm?.FullNameEnglish),
                    FullNameArabic = CleanName(nm?.FullNameArabic),
                    TitleEnglish = CardText.Fix(nm?.TitleEnglish),
                    Gender = nm?.Gender,
                    DateOfBirth = nm?.DateOfBirth,
                    NationalityEnglish = CardText.Fix(nm?.NationalityEnglish),
                    NationalityArabic = CardText.Fix(nm?.NationalityArabic),
                    NationalityCode = nm?.NationalityCode,
                    PlaceOfBirthEnglish = CardText.Fix(nm?.PlaceOfBirthEnglish),

                    AddressEmirate = CardText.Fix(home?.EmirateEnglish),
                    AddressCity = CardText.Fix(home?.CityEnglish),
                    AddressArea = CardText.Fix(home?.AreaEnglish),
                    AddressStreet = CardText.Fix(home?.StreetEnglish),
                    AddressBuilding = CardText.Fix(home?.BuildingNameEnglish),
                    AddressPoBox = home?.PoBox,
                    AddressPhone = home?.LandPhoneNumber,
                    AddressMobile = home?.MobilePhoneNumber,
                    AddressEmail = CardText.Fix(home?.Email),

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
            /* The native DLLs are normally copied beside the executable by the build. If a
               deployment keeps them elsewhere, Toolkit:NativeDirectory points at them -
               added to the search path rather than replacing it, so the app's own
               directory still works. */
            var nativeDirectory = configuration["Toolkit:NativeDirectory"];
            if (!string.IsNullOrWhiteSpace(nativeDirectory) && Directory.Exists(nativeDirectory))
            {
                if (!SetDllDirectory(nativeDirectory))
                {
                    logger.LogWarning("SetDllDirectory failed for {Directory}", nativeDirectory);
                }
            }

            var configDirectory = ResolveConfigDirectory();

            if (configDirectory is null)
            {
                _initialisationError =
                    "Could not find a toolkit config directory. Set Toolkit:ConfigDirectory to the " +
                    "folder containing config_li (and config_ag, for the complete ICP bundle).";
                return false;
            }

            var logDirectory = configuration["Toolkit:LogDirectory"];
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EIDAToolkit", "logs");
            }
            Directory.CreateDirectory(logDirectory);

            /* Newline-separated "key = value", NOT JSON. The quickstart README documents a
               JSON example and the toolkit rejects it with "Invalid or incomplete
               configuration data"; this is the format the working config_ap uses and the
               format the Android sample builds. */
            var config = string.Join('\n',
                $"config_directory = {configDirectory}",
                $"log_directory = {logDirectory}",
                "application_type = APP_INPROC",
                "read_publicdata_offline = true");

            _toolkit = new Toolkit(true, config);
            logger.LogInformation("Toolkit initialised. Config directory: {ConfigDirectory}", configDirectory);
            return true;
        }
        catch (DllNotFoundException ex)
        {
            /* The managed binding loaded but its native dependency did not. Saying which
               files are missing is far more useful than repeating the DLL name. */
            _initialisationError =
                "The native toolkit libraries are not beside the application. " +
                "EIDAToolkit.dll, PCSCLib.dll and the VC++ 2013 runtime (msvcp120.dll, " +
                "msvcr120.dll) must be in the output folder - the build copies them from " +
                "the SDK's quickstart\\64. Original error: " + ex.Message;
            logger.LogError(ex, "Native toolkit libraries missing");
            return false;
        }
        catch (Exception ex)
        {
            _initialisationError = ex.Message;
            logger.LogError(ex, "Toolkit initialisation failed");
            return false;
        }
    }

    /// <summary>
    /// Finds the toolkit config directory: the configured value if it exists, otherwise a
    /// search for one containing config_li.
    ///
    /// The search prefers a directory that also has config_ag, which marks ICP's complete
    /// bundle - the earlier partial delivery carried a licence the Validation Gateway
    /// rejected, and silently picking it back up would resurrect that failure.
    /// </summary>
    private string? ResolveConfigDirectory()
    {
        var configured = configuration["Toolkit:ConfigDirectory"];
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(Path.Combine(configured, "config_li")))
        {
            return configured;
        }

        if (!string.IsNullOrWhiteSpace(configured))
        {
            logger.LogWarning(
                "Toolkit:ConfigDirectory is set to {Configured} but no config_li is there; searching instead.",
                configured);
        }

        var candidates = new List<string>();

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            try
            {
                if (File.Exists(Path.Combine(dir.FullName, "config_li")))
                {
                    candidates.Add(dir.FullName);
                }

                foreach (var child in dir.GetDirectories())
                {
                    if (File.Exists(Path.Combine(child.FullName, "config_li")))
                    {
                        candidates.Add(child.FullName);
                    }

                    foreach (var grandchild in child.GetDirectories())
                    {
                        if (File.Exists(Path.Combine(grandchild.FullName, "config_li")))
                        {
                            candidates.Add(grandchild.FullName);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // A folder we cannot enumerate is simply not a candidate.
            }

            if (candidates.Count > 0) break;
        }

        var complete = candidates.FirstOrDefault(c => File.Exists(Path.Combine(c, "config_ag")));
        var chosen = complete ?? candidates.FirstOrDefault();

        if (chosen is not null && candidates.Count > 1)
        {
            logger.LogInformation(
                "Found {Count} config directories; using {Chosen}{Note}",
                candidates.Count, chosen,
                complete is null ? " (none carried config_ag)" : " (has config_ag)");
        }

        return chosen;
    }

    /// <summary>
    /// Reports a phase and then yields for a moment, so the caller's interface can paint
    /// it before the next blocking native call starts.
    ///
    /// A real delay rather than Task.Yield: yielding can resume before the render batch
    /// has been dispatched, and the phase then never appears. One millisecond per phase
    /// against a read measured in seconds.
    /// </summary>
    private static async Task PhaseAsync(IProgress<string>? progress, string phase)
    {
        if (progress is null) return;
        progress.Report(phase);
        await Task.Delay(1);
    }

    private static readonly string[] LicenceDateFormats =
        ["yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd"];

    /// <summary>
    /// Reads the toolkit's licence expiry string into something showable and a day count.
    ///
    /// The format is the toolkit's own business, and what it actually returns is
    /// <c>2027-04-14+04:00</c> - an ISO date carrying the Gulf offset and no time at all.
    /// So the offset-aware parse is tried first, then the leading ten characters, then it
    /// gives up: an unreadable date is shown verbatim with no count, because a wrong
    /// count is worse than none.
    /// </summary>
    private static (string? Display, int? Days) ReadLicence(string? expiry)
    {
        if (string.IsNullOrWhiteSpace(expiry)) return (null, null);

        var text = expiry.Trim();

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                                    DateTimeStyles.AllowWhiteSpaces, out var offset))
        {
            return Format(DateOnly.FromDateTime(offset.Date));
        }

        // The date on its own, for a suffix the offset-aware parse did not take.
        var head = text.Length >= 10 ? text[..10] : text;
        if (DateOnly.TryParseExact(head, LicenceDateFormats, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out var date))
        {
            return Format(date);
        }

        return (text, null);

        static (string?, int?) Format(DateOnly date)
        {
            // Gulf Standard Time, because that is the day the desk is having.
            var today = DateOnly.FromDateTime(
                DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(4)).DateTime);
            return (date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    date.DayNumber - today.DayNumber);
        }
    }

    /// <summary>
    /// The chip stores names as comma-delimited segments, most of them empty:
    /// "NAYYAR JAWAID,,,,,ALI KHAN," is one person, not seven fields. The Arabic name is
    /// delimited the same way, so it is repaired first and then joined identically.
    /// </summary>
    private static string CleanName(string? value)
    {
        value = CardText.Fix(value);
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

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDllDirectory(string lpPathName);

    public void Dispose()
    {
        try { _toolkit?.Cleanup(); } catch { /* shutting down anyway */ }
        _gate.Dispose();
    }
}
