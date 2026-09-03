using System.Reflection;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Identifies the build that is actually running.
///
/// Exists because "I don't see any change" is otherwise unanswerable: Razor markup is
/// compiled, so a pull without a rebuild leaves the old screens in place, and a rebuild
/// without a browser reload leaves the old stylesheet in place. Both look identical to a
/// change that was never made. The stamp in the sidebar settles it in one glance.
/// </summary>
public static class BuildInfo
{
    /// <summary>Version and build time, for the corner of the screen.</summary>
    public static string Stamp { get; } = Describe();

    /// <summary>
    /// Changes with every build, so app.css and app.js are re-fetched rather than served
    /// from a cache that predates the rebuild.
    /// </summary>
    public static string CacheTag { get; } = Tag();

    private static DateTimeOffset? BuiltUtc()
    {
        try
        {
            // The assembly's own file time. Survives plain `dotnet run`, which is how this
            // is started at a desk; a single-file publish reports no location, hence null.
            var path = typeof(BuildInfo).Assembly.Location;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            return new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
        }
        catch
        {
            return null;
        }
    }

    private static string Describe()
    {
        var assembly = typeof(BuildInfo).Assembly;
        var version =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unversioned";

        // Trim the "+<commit>" the SDK appends when the build knows its source revision.
        var plus = version.IndexOf('+');
        if (plus > 0) version = version[..plus];

        var built = BuiltUtc();
        if (built is null) return version;

        // Gulf Standard Time, like every other time this application shows.
        return $"v{version} · built {built.Value.ToOffset(TimeSpan.FromHours(4)):dd MMM HH:mm} GST";
    }

    private static string Tag()
    {
        var built = BuiltUtc();
        return built is null
            ? Guid.NewGuid().ToString("N")[..8]     // unknown build: never cache
            : built.Value.ToUnixTimeSeconds().ToString();
    }
}
