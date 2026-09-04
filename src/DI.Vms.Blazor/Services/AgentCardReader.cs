using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// What the browser needs in order to talk to the ICP agent on the desk it is running on.
/// Sent to the page; nothing here is a secret, and nothing here is trusted on the way
/// back.
/// </summary>
public sealed class AgentOptions
{
    /// <summary>
    /// <c>true</c> makes eidatoolkit.js use <c>wss://</c>. An HTTPS page cannot open a
    /// plain <c>ws://</c> socket - the browser blocks it as mixed content and the read
    /// never starts - so a server on HTTPS needs this on and the agent's certificate
    /// trusted on the desk.
    /// </summary>
    public bool TlsEnabled { get; set; } = true;

    /// <summary>
    /// The host eidatoolkit.js connects to, on ports 9004, 9005 and 9020 in turn.
    ///
    /// Its own default under TLS is <c>toolkitagent.emiratesid.ae</c>, which resolves to
    /// the loopback address: that is how the agent can present a certificate for a name
    /// a browser will accept while still being the agent on this desk. Under plain
    /// <c>ws://</c> its default is 127.0.0.1.
    ///
    /// Left blank, the SDK's own default for the chosen scheme applies.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// The toolkit configuration handed to the agent, newline-separated
    /// <c>key = value</c> - not JSON; see the README.
    ///
    /// Normally blank: the agent installed by ICAToolkitService.msi carries its own
    /// config, and the paths in it are the desk's, not the server's. It is settable
    /// because a desk whose agent has no config of its own needs one from somewhere.
    /// </summary>
    public string? ToolkitConfig { get; set; }

    /// <summary>Turns on eidatoolkit.js's own console logging on the desk.</summary>
    public bool DebugEnabled { get; set; }

    /// <summary>
    /// SHA-1 thumbprints of the certificates whose signature over a response we accept,
    /// upper case, no spaces.
    ///
    /// This is the control that makes an agent read trustworthy, and it is the one thing
    /// about option B that cannot be skipped: the certificate that signed the response
    /// travels inside the response, so a browser can send a document it signed itself
    /// and the signature will check out perfectly. Only the identity of the signer
    /// separates a real card from a fabricated one.
    ///
    /// Empty means no signer is pinned. Reads are still accepted - refusing them would
    /// leave a deployment with no working path at all - but each one is marked with a
    /// warning, and the thumbprint that signed it is logged so it can be pinned here.
    /// </summary>
    public string[] TrustedSignerThumbprints { get; set; } = [];

    /// <summary>
    /// Just the part the browser needs. The pinned thumbprints and the age limit are the
    /// server's business and are checked there; sending them to the page would only
    /// suggest the page had a say in them.
    /// </summary>
    public AgentBrowserOptions ForBrowser() => new()
    {
        TlsEnabled = TlsEnabled,
        HostName = HostName,
        ToolkitConfig = ToolkitConfig,
        DebugEnabled = DebugEnabled,
    };

    /// <summary>
    /// Whether a response whose signature does not verify is refused.
    ///
    /// True is the right default and the reason agent mode is safe at all. It is settable
    /// because the alternative to a deployment that refuses every read is not a safer
    /// deployment - it is card reading switched off and every visitor typed in by hand,
    /// which records the same data with no check on it whatsoever.
    ///
    /// Set false only deliberately, and knowing what is given up: a read from a browser
    /// is then a claim the server cannot test. Every such read is marked on screen and in
    /// the log, and the entry is saved with the warning on it.
    /// </summary>
    public bool RequireSignature { get; set; } = true;

    /// <summary>
    /// How stale a gateway timestamp may be. A response is single-use by request ID
    /// already; this is the second lock, for a replay that arrives before the request ID
    /// it was issued for has expired.
    /// </summary>
    public TimeSpan MaximumResponseAge { get; set; } = TimeSpan.FromMinutes(10);
}

/// <summary>What eidatoolkit.js is configured with, in the browser.</summary>
public sealed class AgentBrowserOptions
{
    public bool TlsEnabled { get; init; }
    public string? HostName { get; init; }
    public string? ToolkitConfig { get; init; }
    public bool DebugEnabled { get; init; }
}

/// <summary>What card-agent.js reports back about the desk it is running on.</summary>
public sealed class AgentProbe
{
    public bool AgentAvailable { get; set; }
    public int? AgentPort { get; set; }
    public string? Detail { get; set; }
    public string? ToolkitVersion { get; set; }
    public string? ReaderName { get; set; }
    public bool CardPresent { get; set; }

    /// <summary>
    /// Whatever the agent said about the licence, undecoded.
    ///
    /// Declared as raw JSON because the toolkit's getLicenseExpiryDate does not resolve
    /// with a date - it resolves with a whole response object, and the date is a property
    /// on it. Typed as a string it took the entire reader status down with
    /// "The JSON value could not be converted to System.String", so a desk with a working
    /// reader was told it had none, over a line of small print about a licence.
    ///
    /// An optional display field does not get to do that. Read it through
    /// <see cref="LicenceExpiryText"/>, which returns null rather than throwing on
    /// anything it does not recognise.
    /// </summary>
    public JsonElement? LicenceExpiry { get; set; }

    /// <summary>The licence date as text, or null if it did not come back as anything.</summary>
    public string? LicenceExpiryText()
    {
        if (LicenceExpiry is not { } value) return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),

            // The whole response object. ICP's own sample reads .expirydate off it.
            JsonValueKind.Object => Property(value, "expirydate")
                ?? Property(value, "expiryDate")
                ?? Property(value, "expiry_date"),

            _ => null,
        };

        static string? Property(JsonElement element, string name) =>
            element.TryGetProperty(name, out var found) && found.ValueKind is not JsonValueKind.Null
                ? (found.ValueKind == JsonValueKind.String ? found.GetString() : found.ToString())
                : null;
    }
}

/// <summary>The signed response, and nothing else - see card-agent.js.</summary>
public sealed class AgentReadResult
{
    public string? Xml { get; set; }
}

/// <summary>
/// The server half of a read performed by the ICP agent on the reception desk.
///
/// The in-process path can trust what the toolkit hands it, because the toolkit ran here.
/// This path cannot trust anything: the response arrives over the circuit from a browser,
/// and a browser is a program the visitor's side of the desk can replace. So the server
/// issues the request ID, and the response is only a card once
///
///   1. it carries that exact request ID, which is single-use and expires,
///   2. its XML signature verifies, against a signer we pinned rather than one it
///      nominated, and
///   3. the gateway's timestamp on it is recent.
///
/// Every field then comes out of the signed XML - never out of anything the browser
/// parsed for us, which would make the signature decorative.
/// </summary>
public sealed class AgentCardReader(AgentOptions options, ILogger<AgentCardReader> logger)
{
    /// <summary>
    /// Issued request IDs and when they expire. Bounded by the expiry sweep, and in
    /// memory on purpose: a request ID outliving a restart has no value, and one desk's
    /// read is never completed by another server.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTimeOffset> _outstanding = new(StringComparer.Ordinal);

    private static readonly TimeSpan RequestLifetime = TimeSpan.FromMinutes(5);

    public AgentOptions Options { get; } = options;

    /// <summary>
    /// Starts a read. The ID goes to the browser, which passes it to the agent, which
    /// asks the gateway to stamp it into the signed response.
    /// </summary>
    public string BeginRead()
    {
        Sweep();

        /* The same shape the in-process path uses. It has to be unpredictable: a request
           ID a browser could guess ahead of time would let it prepare a response before
           being asked for one. */
        var requestId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        _outstanding[requestId] = DateTimeOffset.UtcNow + RequestLifetime;
        return requestId;
    }

    /// <summary>
    /// Turns the agent's response into a card, or throws saying why it is not one.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The response failed one of the checks. The message is written to be shown at the
    /// desk, because the attendant is the one who has to decide what to do next.
    /// </exception>
    public CardData Complete(string requestId, string? responseXml)
    {
        if (string.IsNullOrWhiteSpace(responseXml))
        {
            throw new InvalidOperationException("The agent returned no response for the card.");
        }

        if (responseXml.Length > CardResponseParser.MaxXmlLength)
        {
            throw new InvalidOperationException("The agent's response is too large to be a card read.");
        }

        /* Removed, not merely checked: a response is good for exactly one visitor entry,
           so a second attempt with the same document fails here rather than producing a
           duplicate check-in. */
        if (!_outstanding.TryRemove(requestId, out var expires))
        {
            throw new InvalidOperationException(
                "This read has already been used, or took too long. Read the card again.");
        }

        if (DateTimeOffset.UtcNow > expires)
        {
            throw new InvalidOperationException("This read took too long to come back. Read the card again.");
        }

        var responseRequestId = CardResponseParser.ReadRequestId(responseXml);
        if (!string.Equals(requestId, responseRequestId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Agent response carried request ID {Returned}, expected {Expected}.",
                responseRequestId ?? "(none)", requestId);

            throw new InvalidOperationException(
                "The response does not belong to this read. Read the card again.");
        }

        var (valid, thumbprint, subject) = CardResponseParser.VerifySignature(responseXml);

        if (!valid)
        {
            /* Structure only - algorithms, what the signature covers, who signed it. No
               name, no ID number, no photograph, so this is safe in a log and safe to
               send to ICP. "The signature did not verify" is not a diagnosis on its own. */
            logger.LogWarning(
                "Agent response failed signature verification. {Description}",
                CardResponseParser.Describe(responseXml));
        }

        var warning = CheckSigner(valid, thumbprint, subject, responseXml);

        var age = CardResponseParser.ReadTimestamp(responseXml) is { } stamped
            ? DateTimeOffset.UtcNow - stamped
            : (TimeSpan?)null;

        if (age > Options.MaximumResponseAge)
        {
            throw new InvalidOperationException(
                $"The response was produced {age.Value.TotalMinutes:F0} minutes ago, so it is not this " +
                "card read. Read the card again.");
        }

        var card = CardResponseParser.Parse(responseXml);
        card.SignatureWarning = warning;

        logger.LogInformation(
            "Agent read accepted. Signer {Subject}, photograph {Photo} bytes, signature {Signature} bytes.",
            subject ?? "(none)", card.Photo?.Length ?? 0, card.CardSignature?.Length ?? 0);

        return card;
    }

    /// <summary>
    /// Decides what an unpinned or unexpected signer means. Fails closed once a signer
    /// has been pinned, and says so loudly until one has been.
    /// </summary>
    private string? CheckSigner(bool valid, string? thumbprint, string? subject, string responseXml)
    {
        if (!valid)
        {
            /* The two failures are different problems and want different answers, so the
               desk is told which one it has rather than one message covering both. */
            var detail = CardResponseParser.HasSignature(responseXml)
                ? "The response carries a signature that does not verify."
                : "The response carries no signature at all.";

            if (Options.RequireSignature)
            {
                throw new InvalidOperationException(
                    detail + " This read cannot be shown to be genuine, so it has not been accepted. " +
                    "The server log says what is wrong with it.");
            }

            logger.LogWarning(
                "Accepting an unverified agent read because Toolkit:Agent:RequireSignature is false. {Detail}",
                detail);

            return "This read was not verified. " + detail;
        }

        var pinned = Options.TrustedSignerThumbprints
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Normalise)
            .ToArray();

        if (pinned.Length == 0)
        {
            /* Logged with the exact configuration line to add, because the alternative -
               "pin the certificate" with no thumbprint - is advice nobody can act on. */
            logger.LogWarning(
                "No signer is pinned, so this read was accepted on a signature that verifies against a " +
                "certificate inside the response itself. Signer: {Subject}. To pin it, set " +
                "Toolkit:Agent:TrustedSignerThumbprints to [ \"{Thumbprint}\" ].",
                subject ?? "(none)", thumbprint ?? "(none)");

            return "This read was not checked against a known signer. See docs/deployment.md.";
        }

        if (thumbprint is null || !pinned.Contains(Normalise(thumbprint)))
        {
            logger.LogError(
                "Agent response was signed by {Subject} ({Thumbprint}), which is not a pinned signer.",
                subject ?? "(none)", thumbprint ?? "(none)");

            throw new InvalidOperationException(
                "The response was signed by a certificate this system does not trust, so it is not a " +
                "genuine card read.");
        }

        return null;
    }

    private static string Normalise(string thumbprint) =>
        thumbprint.Replace(" ", string.Empty).Replace(":", string.Empty).ToUpperInvariant();

    private void Sweep()
    {
        if (_outstanding.Count < 64) return;

        var now = DateTimeOffset.UtcNow;
        foreach (var (key, expires) in _outstanding)
        {
            if (now > expires) _outstanding.TryRemove(key, out _);
        }
    }
}
