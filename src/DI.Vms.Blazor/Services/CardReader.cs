using System.Globalization;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Reading a card on this machine, or honestly reporting that this machine cannot.
///
/// The interface exists so the toolkit is optional at compile time, not just at runtime.
/// ICP's binding is a .NET Framework assembly that P/Invokes native Windows DLLs, and a
/// project that references it is a project that only builds and runs on Windows. In agent
/// mode the server never touches any of it - the card is read by the client and what
/// arrives is signed XML - so on a Linux host the implementation behind this is simply the
/// one that says there is no reader here, and CardReaderService.cs is not compiled at all.
///
/// See DI.Vms.Blazor.csproj, the VmsAgentOnly property.
/// </summary>
public interface ICardReader
{
    /// <summary>
    /// Whether this host reads cards in this process. False in agent mode, false where
    /// cards are typed in, and false always on a build without the toolkit.
    /// </summary>
    bool Enabled { get; }

    /// <summary>Whether a card could be read right now, and why not when it could not.</summary>
    Task<ReaderState> GetStateAsync();

    /// <summary>Reads the card in the reader, reporting each phase as it goes.</summary>
    Task<CardData> ReadAsync(IProgress<string>? progress = null);
}

/// <summary>
/// The reader on a host that has none, which is every host in agent mode and every Linux
/// host whatever the mode.
///
/// It answers rather than throws. The screens already handle "no reader here" - it is the
/// same answer a reception PC gives when Toolkit:Mode is Agent - so a host without the
/// toolkit compiled in is not a special case anywhere above this.
/// </summary>
public sealed class NoCardReader(CardCaptureOptions capture) : ICardReader
{
    public bool Enabled => false;

    public Task<ReaderState> GetStateAsync() => Task.FromResult(new ReaderState
    {
        Available = false,
        Detail = capture.Mode == CardCaptureMode.InProcess
            /* Configured to read in-process on a host that cannot. Worth saying plainly,
               because the setting looks right and the behaviour will not be. */
            ? "Toolkit:Mode is InProcess, but this build has no card reader compiled in - " +
              "it is the portable build, which runs on Linux and has no ICP toolkit. Set " +
              "Toolkit:Mode to Agent so the desk's own reader is used, or deploy the Windows " +
              "build to a machine with a reader. See docs/deployment.md."
            : "This host does not read cards itself. Toolkit:Mode decides how it reads " +
              "them; see docs/deployment.md.",
    });

    public Task<CardData> ReadAsync(IProgress<string>? progress = null) =>
        throw new InvalidOperationException(
            "This host has no card reader. A card is read by the desk's browser through " +
            "ICP's agent, or by the tablet, and verified here - see AgentCardReader.");
}

/// <summary>
/// The toolkit's licence expiry, as something showable.
///
/// Here rather than on CardReaderService because the diagnostics panel shows it for an
/// agent-mode read too, where the licence belongs to the agent on the desk and this
/// process has no toolkit at all - possibly no toolkit compiled in.
/// </summary>
public static class ToolkitLicence
{
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
    public static (string? Display, int? Days) Read(string? expiry)
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
}
