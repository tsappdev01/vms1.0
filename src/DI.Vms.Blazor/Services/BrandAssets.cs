namespace DI.Vms.Blazor.Services;

/// <summary>
/// Finds the official brand artwork if it has been supplied.
///
/// The lockup is otherwise composed in markup, which is a rendition rather than the
/// asset. Dropping the real file into wwwroot/brand ends that: it is picked up on the
/// next start and used everywhere the lockup appears, with no code change.
/// </summary>
public static class BrandAssets
{
    /// <summary>
    /// Web path to the supplied lockup, or null when none has been dropped in and the
    /// composed rendition should be used instead.
    /// </summary>
    public static string? LogoPath { get; private set; }

    /// <summary>
    /// Web path to a photograph of the card reader, or null when none has been supplied
    /// and the drawn illustration should be used.
    ///
    /// The drawn one is a diagram of a reader; a photograph of the actual ACR39U with a
    /// card going into it tells a visitor which way round the card goes far better than
    /// any drawing will. Same drop-in rule as the lockup, and for the same reason: the
    /// hand-drawn version of the logo was never quite the logo.
    /// </summary>
    public static string? ReaderImagePath { get; private set; }

    /// <summary>Called once at startup, when the content root is known.</summary>
    public static void Locate(string? webRootPath, ILogger logger)
    {
        // Null under some hosting layouts. Nothing to search, and a brand asset is not
        // worth failing a start over.
        if (string.IsNullOrEmpty(webRootPath))
        {
            logger.LogWarning("No web root; brand lockup will use the composed rendition.");
            return;
        }

        // In preference order: a vector scales, so it wins over the raster forms.
        LogoPath = FirstPresent(webRootPath, ["di-logo.svg", "di-logo.png", "di-logo.webp", "di-logo.jpg"]);

        if (LogoPath is not null)
        {
            logger.LogInformation("Using supplied brand lockup {Path}.", LogoPath);
        }
        else
        {
            logger.LogInformation(
                "No brand lockup in wwwroot/brand; using the composed rendition. Drop " +
                "di-logo.svg (or .png) there to use the official asset instead.");
        }

        /* A photograph, so raster first here - and webp before png because this one is a
           product shot, where the size difference is worth having on a desk browser. */
        ReaderImagePath = FirstPresent(webRootPath, ["reader.webp", "reader.png", "reader.jpg", "reader.svg"]);

        if (ReaderImagePath is not null)
        {
            logger.LogInformation("Using supplied reader photograph {Path}.", ReaderImagePath);
        }
        else
        {
            logger.LogInformation(
                "No reader photograph in wwwroot/brand; using the drawn illustration. Drop " +
                "reader.png (or .webp) there to show the real reader instead.");
        }
    }

    private static string? FirstPresent(string webRootPath, string[] candidates)
    {
        foreach (var name in candidates)
        {
            if (File.Exists(Path.Combine(webRootPath, "brand", name)))
            {
                return $"brand/{name}";
            }
        }

        return null;
    }
}
