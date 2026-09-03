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
        string[] candidates = ["di-logo.svg", "di-logo.png", "di-logo.webp", "di-logo.jpg"];

        foreach (var name in candidates)
        {
            if (!File.Exists(Path.Combine(webRootPath, "brand", name))) continue;

            LogoPath = $"brand/{name}";
            logger.LogInformation("Using supplied brand lockup {Path}.", LogoPath);
            return;
        }

        logger.LogInformation(
            "No brand lockup in wwwroot/brand; using the composed rendition. Drop " +
            "di-logo.svg (or .png) there to use the official asset instead.");
    }
}
