using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Reads a card from the ICP Validation Gateway response XML, and checks the signature
/// over it.
///
/// Both read paths end up here. The in-process path (<see cref="CardReaderService"/>)
/// already has the fields as managed objects and uses this only to check the signature;
/// the agent path (<see cref="AgentCardReader"/>) has nothing but the XML, because
/// nothing else a browser sends can be believed.
///
/// The element names are taken from the vendor's own JavaScript SDK
/// (<c>lib/web/eidatoolkit.js</c>: <c>NonModifiablePublicData</c>, <c>HomeAddress</c> and
/// <c>CardPublicData</c> read exactly these paths out of the same document), not guessed
/// from a sample response.
/// </summary>
internal static class CardResponseParser
{
    /// <summary>
    /// A signed response with a photograph is tens of kilobytes. The cap is generous, and
    /// exists because in the agent path the length is chosen by the browser.
    /// </summary>
    public const int MaxXmlLength = 8 * 1024 * 1024;

    /// <summary>
    /// Parses the response into the same shape the in-process read produces, so
    /// everything downstream - the screens, the save, the report - has one path.
    /// </summary>
    public static CardData Parse(string xml)
    {
        var document = Load(xml);

        var message = Find(document, "Message")
            ?? throw new InvalidOperationException("The response has no Message element.");

        var header = Find(message, "Header");
        var body = Find(message, "Body")
            ?? throw new InvalidOperationException("The response has no Body element.");

        var publicData = Find(body, "PublicData")
            ?? throw new InvalidOperationException(
                "The response carries no PublicData. The card may not have been read.");

        var nm = Find(publicData, "NonModifiableData");
        var home = Find(publicData, "HomeAddress");

        return new CardData
        {
            /* From the header: the gateway echoes both there, and the header is the part
               the request ID is checked against. */
            IdNumber = Text(header, "IDNumber") ?? string.Empty,
            CardNumber = Text(header, "CardNumber") ?? Text(publicData, "CardNumber"),

            Photo = Base64(publicData, "CardHolderPhoto"),

            IdType = Text(nm, "IdType"),
            IssueDate = Text(nm, "IssueDate"),
            ExpiryDate = Text(nm, "ExpiryDate"),
            FullNameEnglish = CleanName(Text(nm, "FullNameEnglish")),
            FullNameRaw = CardText.Fix(Text(nm, "FullNameEnglish")),
            FullNameArabic = CleanName(Text(nm, "FullNameArabic")),
            TitleEnglish = CardText.Fix(Text(nm, "TitleEnglish")),
            Gender = Text(nm, "Gender"),
            DateOfBirth = Text(nm, "DateOfBirth"),
            NationalityEnglish = CardText.Fix(Text(nm, "NationalityEnglish")),
            NationalityArabic = CardText.Fix(Text(nm, "NationalityArabic")),
            NationalityCode = Text(nm, "NationalityCode"),
            PlaceOfBirthEnglish = CardText.Fix(Text(nm, "PlaceOfBirthEnglish")),

            /* The address element names are the document's, which are not the managed
               binding's: EmiratesDescEnglish rather than EmirateEnglish, POBOX rather
               than PoBox. */
            AddressEmirate = CardText.Fix(Text(home, "EmiratesDescEnglish")),
            AddressCity = CardText.Fix(Text(home, "CityDescEnglish")),
            AddressArea = CardText.Fix(Text(home, "AreaDescEnglish")),
            AddressStreet = CardText.Fix(Text(home, "StreetEnglish")),
            AddressBuilding = CardText.Fix(Text(home, "BuildingNameEnglish")),
            AddressPoBox = Text(home, "POBOX"),
            AddressPhone = Text(home, "ResidentPhoneNumber"),
            AddressMobile = Text(home, "MobilePhoneNumber"),
            AddressEmail = CardText.Fix(Text(home, "Email")),
        };
    }

    /// <summary>The request ID the response was produced for, or null.</summary>
    public static string? ReadRequestId(string xml)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(xml), SafeReaderSettings);
            return reader.ReadToFollowing("RequestID") ? reader.ReadElementContentAsString() : null;
        }
        catch (XmlException)
        {
            return null;
        }
    }

    /// <summary>The gateway's own timestamp, when it is present and parseable.</summary>
    public static DateTimeOffset? ReadTimestamp(string xml)
    {
        try
        {
            var document = Load(xml);
            var value = Text(Find(document, "Header"), "Timestamp");

            return DateTimeOffset.TryParse(
                value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed)
                ? parsed
                : null;
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Checks the XML signature, and reports which certificate signed it.
    ///
    /// <paramref name="thumbprint"/> is the caller's business, and the difference between
    /// the two read paths. A bare <c>CheckSignature()</c> proves only that the document
    /// has not changed since whoever signed it did - and the signer's own certificate is
    /// inside the document, so anyone can produce a document that passes. In-process that
    /// is a fine integrity check, because the bytes never left our process. From a
    /// browser it proves nothing at all until the certificate is one we expected, which
    /// is why the thumbprint comes back out for the caller to insist on.
    /// </summary>
    public static (bool Valid, string? Thumbprint, string? Subject) VerifySignature(string xml)
    {
        try
        {
            /* Tried both ways round. Canonicalisation is sensitive to whether the
               parser kept insignificant whitespace, and which way works depends on how
               the gateway serialised the document it signed - so this does not guess. */
            var attempt = Check(xml, preserveWhitespace: true);
            return attempt.Valid ? attempt : Check(xml, preserveWhitespace: false);
        }
        catch (Exception ex) when (ex is XmlException or CryptographicException or InvalidOperationException)
        {
            return (false, null, null);
        }
    }

    private static (bool Valid, string? Thumbprint, string? Subject) Check(string xml, bool preserveWhitespace)
    {
        var document = Load(xml, preserveWhitespace);

        var nodes = document.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
        if (nodes.Count == 0) return (false, null, null);

        var signedXml = new SignedXml(document);
        signedXml.LoadXml((XmlElement)nodes[0]!);

        var certificate = signedXml.KeyInfo
            .OfType<KeyInfoX509Data>()
            .SelectMany(data => data.Certificates?.OfType<X509Certificate2>() ?? [])
            .FirstOrDefault();

        /* Verified against the certificate in the document when there is one, so the
           thumbprint reported is the key the signature actually checks out against and
           not merely a certificate that happened to be attached alongside it. */
        var valid = certificate is null
            ? signedXml.CheckSignature()
            : signedXml.CheckSignature(certificate, verifySignatureOnly: true);

        return (valid, certificate?.Thumbprint, certificate?.Subject);
    }

    /// <summary>
    /// Describes the document's signature, structurally: what algorithms it uses, what it
    /// covers, who signed it. Nothing off the card - no name, no number, no photograph -
    /// so it is safe to log and safe to paste into a support ticket.
    ///
    /// This exists because "the signature did not verify" is not a diagnosis. It can mean
    /// no signature at all, an algorithm this runtime will not use, a reference to an
    /// element the verifier cannot resolve, or a genuine mismatch - and only the first
    /// and last of those are about the card.
    /// </summary>
    public static string Describe(string xml)
    {
        try
        {
            var document = Load(xml, preserveWhitespace: true);
            var root = document.DocumentElement?.LocalName ?? "(none)";

            var nodes = document.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            if (nodes.Count == 0)
            {
                return $"root={root}, length={xml.Length}, signature=ABSENT";
            }

            var signedXml = new SignedXml(document);
            signedXml.LoadXml((XmlElement)nodes[0]!);

            var info = signedXml.SignedInfo;

            var references = info is null ? "(none)" : string.Join(" | ", info.References
                .OfType<Reference>()
                .Select(reference =>
                    $"uri='{reference.Uri}' digest={Tail(reference.DigestMethod)} " +
                    $"transforms=[{Transforms(reference.TransformChain)}]"));

            var certificate = signedXml.KeyInfo
                .OfType<KeyInfoX509Data>()
                .SelectMany(data => data.Certificates?.OfType<X509Certificate2>() ?? [])
                .FirstOrDefault();

            return $"root={root}, length={xml.Length}, signatures={nodes.Count}, " +
                   $"c14n={Tail(info?.CanonicalizationMethod)}, method={Tail(info?.SignatureMethod)}, " +
                   $"refs=[{references}], " +
                   $"key={(certificate is null ? "no certificate in KeyInfo" : certificate.Subject)}";
        }
        catch (Exception ex)
        {
            return $"could not be described: {ex.GetType().Name}: {ex.Message}";
        }

        // The algorithm URIs are long and all share a prefix; the tail is the identifying part.
        static string Tail(string? uri) =>
            string.IsNullOrEmpty(uri) ? "(none)" : uri[(uri.LastIndexOf('#') + 1)..];

        /* TransformChain has Count and an indexer but does not implement IEnumerable, so
           it is walked by index rather than with LINQ. */
        static string Transforms(TransformChain chain)
        {
            var algorithms = new string[chain.Count];
            for (var i = 0; i < chain.Count; i++)
            {
                algorithms[i] = Tail(chain[i].Algorithm);
            }

            return string.Join(",", algorithms);
        }
    }

    /// <summary>True when the document carries an XML signature at all.</summary>
    public static bool HasSignature(string xml)
    {
        try
        {
            return Load(xml).GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Count > 0;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    /// <summary>
    /// The chip's comma-delimited segments joined into a readable name.
    /// "NAYYAR JAWAID,,,,,ALI KHAN," is one person, not seven fields; the empty positions
    /// carry given/middle/family meaning, which is why the raw value is kept as well.
    /// </summary>
    public static string CleanName(string? value)
    {
        value = CardText.Fix(value);
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var joined = string.Join(' ', value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => part.Length > 0));

        return joined.Length > 0 ? joined : value.Trim();
    }

    private static readonly XmlReaderSettings SafeReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    /// <summary>
    /// Loads with the resolver off and DTDs prohibited. This document is untrusted in the
    /// agent path, and a parser that will fetch an external entity turns a card read into
    /// a request made by the server on the browser's behalf.
    /// </summary>
    private static XmlDocument Load(string xml, bool preserveWhitespace = false)
    {
        var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = preserveWhitespace };

        using var reader = XmlReader.Create(new StringReader(xml), SafeReaderSettings);
        document.Load(reader);
        return document;
    }

    /* Local-name lookups throughout: the gateway's documents have carried a default
       namespace in some responses and none in others, and a namespace-qualified XPath
       that guesses wrong finds nothing and reports it as an empty card. */
    private static XmlNode? Find(XmlNode? scope, string localName) =>
        scope?.SelectSingleNode($".//*[local-name()='{localName}']");

    private static string? Text(XmlNode? scope, string localName)
    {
        var value = Find(scope, localName)?.InnerText;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static byte[]? Base64(XmlNode? scope, string localName)
    {
        var value = Text(scope, localName);
        if (value is null) return null;

        try
        {
            // Base64 in signed XML is normally wrapped across lines.
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            /* Not fatal: a photograph that will not decode costs a placeholder, and
               refusing the whole read over it would turn a cosmetic problem into a
               visitor who cannot check in. */
            return null;
        }
    }
}
