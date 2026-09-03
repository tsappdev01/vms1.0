namespace DI.Vms.Blazor.Services;

/// <summary>
/// Where the card reader is, relative to this process. It is the deployment's central
/// decision, so it is one setting rather than something inferred.
/// </summary>
public enum CardCaptureMode
{
    /// <summary>
    /// The reader is on this machine and the toolkit is called in-process. The app runs
    /// at the reception desk; this is the mode the system was built for.
    /// </summary>
    InProcess,

    /// <summary>
    /// The reader is on the desk and this process is on a server. The browser talks to
    /// ICP's agent on the desk over a WebSocket and posts the signed response back, which
    /// the server verifies before believing a word of it. See
    /// <see cref="AgentCardReader"/>.
    /// </summary>
    Agent,

    /// <summary>
    /// No card reading. Visitor details are typed in. What a server with no agent rollout
    /// behind it can actually offer, and honest about it.
    /// </summary>
    Off,
}

/// <summary>
/// The capture settings, resolved once at startup so every screen and both readers agree
/// on what this host is.
/// </summary>
public sealed class CardCaptureOptions
{
    public CardCaptureMode Mode { get; init; } = CardCaptureMode.InProcess;

    public AgentOptions Agent { get; init; } = new();

    public bool ReadsCards => Mode is not CardCaptureMode.Off;

    /// <summary>
    /// Reads <c>Toolkit:Mode</c>, falling back to the older <c>Toolkit:Enabled</c> so a
    /// deployment that set that keeps working.
    /// </summary>
    public static CardCaptureOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("Toolkit");
        var configured = section["Mode"];

        CardCaptureMode mode;

        if (string.IsNullOrWhiteSpace(configured))
        {
            mode = section.GetValue("Enabled", true) ? CardCaptureMode.InProcess : CardCaptureMode.Off;
        }
        else if (!Enum.TryParse(configured, ignoreCase: true, out mode))
        {
            /* Failing is better than falling back: a typo would otherwise put a host into
               a mode that cannot work and leave the reader to take the blame. */
            throw new InvalidOperationException(
                $"Toolkit:Mode is '{configured}', which is not one of InProcess, Agent or Off.");
        }

        return new CardCaptureOptions
        {
            Mode = mode,
            Agent = section.GetSection("Agent").Get<AgentOptions>() ?? new AgentOptions(),
        };
    }

    /// <summary>
    /// Says at startup what this host is, and what it is missing. Separate from reading
    /// the configuration so it can run where there is a real logger, and so the
    /// deployment-time warning reaches the person who can act on it - at the desk, a read
    /// is far too late to be told the signer was never pinned.
    /// </summary>
    public void LogTo(ILogger logger)
    {
        logger.LogInformation("Card capture mode: {Mode}.", Mode);

        if (Mode == CardCaptureMode.Agent && Agent.TrustedSignerThumbprints.Length == 0)
        {
            logger.LogWarning(
                "Agent mode with no pinned signer. A card read arrives from a browser, and the " +
                "certificate that signed it travels inside it, so until " +
                "Toolkit:Agent:TrustedSignerThumbprints is set a forged response cannot be told from a " +
                "real one. The first read logs the thumbprint to pin. See docs/deployment.md.");
        }

        if (Mode == CardCaptureMode.Agent && !Agent.TlsEnabled)
        {
            logger.LogWarning(
                "Agent mode with TLS off. The browser will open a plain ws:// socket to the agent, " +
                "which a page served over HTTPS is not allowed to do - the read will fail as mixed " +
                "content. Set Toolkit:Agent:TlsEnabled true unless the app itself is on plain HTTP.");
        }
    }
}
