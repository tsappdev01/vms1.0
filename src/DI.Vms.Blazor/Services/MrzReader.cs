namespace DI.Vms.Blazor.Services;

/// <summary>
/// Whether the desk is offered the digital-card scan, and where the recogniser comes from.
///
/// The recognition runs in the browser, on Tesseract compiled to WebAssembly. That is a
/// decision about cost and about deployment, in that order: it is free and it stays free
/// however many visitors arrive, where a cloud OCR service is billed per photograph; and it
/// needs nothing installed on a host that runs on App Service's built-in .NET image, where
/// a native Tesseract cannot be put.
///
/// What it costs instead is accuracy. The image is a photograph of a phone screen - glare,
/// and the moiré of one pixel grid photographed through another - and a small local model
/// is where that bites first. The answer to that is not a better recogniser but
/// <see cref="MachineReadableZone"/>: every field is checked by arithmetic, so a misread is
/// refused rather than recorded, and the officer is asked to take the photograph again.
/// </summary>
public sealed class DigitalCardOptions
{
    public const string SectionName = "DigitalCard";

    /// <summary>
    /// Off by default. A deployment that has not decided whether a photographed card is
    /// acceptable evidence should not find the button on the screen one morning.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Where tesseract.js is served from.
    ///
    /// A public CDN by default, because that needs nothing of whoever deploys this. On a
    /// network that will not reach one - and Dubai Investments' does pass through Zscaler -
    /// put the files under wwwroot/lib/tesseract and set this to /lib/tesseract. Then the
    /// engine is served by this application, over the same connection as the page, and
    /// works with no outbound access at all.
    /// </summary>
    public string EngineBaseUrl { get; init; } = "https://cdn.jsdelivr.net/npm/tesseract.js@5/dist";

    /// <summary>
    /// Where the language data is fetched from. Separate from the engine because the engine
    /// is a few hundred kilobytes and this is several megabytes; a deployment may reasonably
    /// host one and not the other.
    /// </summary>
    public string TrainedDataUrl { get; init; } = "https://tessdata.projectnaptha.com/4.0.0";

    /// <summary>
    /// A photograph from a phone camera is several megabytes of which almost none is the
    /// zone. The browser scales it down before recognising; this is the ceiling on what it
    /// will accept at all, so a video file chosen by mistake fails quickly and clearly.
    /// </summary>
    public int MaximumBytes { get; init; } = 12 * 1024 * 1024;

    public static DigitalCardOptions FromConfiguration(IConfiguration config)
    {
        var section = config.GetSection(SectionName);

        return new DigitalCardOptions
        {
            Enabled = section.GetValue("Enabled", false),
            EngineBaseUrl = section["EngineBaseUrl"] is { Length: > 0 } engine
                ? engine.TrimEnd('/')
                : "https://cdn.jsdelivr.net/npm/tesseract.js@5/dist",
            TrainedDataUrl = section["TrainedDataUrl"] is { Length: > 0 } data
                ? data.TrimEnd('/')
                : "https://tessdata.projectnaptha.com/4.0.0",
            MaximumBytes = section.GetValue("MaximumBytes", 12 * 1024 * 1024),
        };
    }
}

/// <summary>
/// Finds the machine-readable zone in whatever the recogniser returned.
///
/// The recogniser returns every line it found, and the back of a card also carries the card
/// number, the occupation, the employer and a notice about returning it to a police station.
/// The zone is picked out by trying each run of three lines and keeping the first whose
/// check digits hold - which is the only test that matters anyway, so there is no separate
/// "does this look like an MRZ" heuristic here to get subtly wrong.
/// </summary>
public static class MrzFinder
{
    public static MachineReadableZone.Result Find(string? recognised)
    {
        if (string.IsNullOrWhiteSpace(recognised)) return MachineReadableZone.Parse(null);

        var candidates = recognised
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => new string(line.ToUpperInvariant()
                .Where(c => MachineReadableZone.Alphabet.Contains(c)).ToArray()))
            .Where(line => line.Length >= 20)
            .ToList();

        MachineReadableZone.Result? closest = null;

        for (var i = 0; i + 3 <= candidates.Count; i++)
        {
            var attempt = MachineReadableZone.Parse(string.Join('\n', candidates.Skip(i).Take(3)));
            if (attempt.Ok) return attempt;

            closest ??= attempt;
        }

        /* Nothing held.
           
           Which refusal to show matters more than it looks. Handing back every line the
           recogniser found makes Normalise see five or six lines and say "that does not look
           like the three lines from the back of the card" - which reads as though the officer
           photographed the wrong side, when they photographed the right side and it was
           misread. So when three lines were found and simply did not add up, the refusal from
           that attempt is the honest one. */
        return closest ?? MachineReadableZone.Parse(string.Join('\n', candidates));
    }
}
