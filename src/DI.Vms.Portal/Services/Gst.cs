namespace DI.Vms.Portal.Services;

/// <summary>
/// The API speaks UTC; the portal displays Gulf Standard Time. GST has no daylight
/// saving, so a fixed offset is safe and avoids depending on the host's tz database.
/// </summary>
public static class Gst
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(4);

    public static string Time(DateTimeOffset? instant) =>
        instant is null ? "—" : instant.Value.ToOffset(Offset).ToString("HH:mm");

    public static string Date(DateTimeOffset? instant) =>
        instant is null ? "—" : instant.Value.ToOffset(Offset).ToString("dd MMM yyyy");

    public static string Date(DateOnly? date) =>
        date is null ? "—" : date.Value.ToString("dd MMM yyyy");

    public static string DateTime(DateTimeOffset? instant) =>
        instant is null ? "—" : instant.Value.ToOffset(Offset).ToString("dd MMM yyyy HH:mm");

    /// <summary>Elapsed time as "1h 36m", per the BRD's duration example.</summary>
    public static string Duration(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is null) return "—";
        var span = (to ?? DateTimeOffset.UtcNow) - from.Value;
        if (span < TimeSpan.Zero) return "—";
        return span.TotalHours >= 1
            ? $"{(int)span.TotalHours}h {span.Minutes}m"
            : $"{span.Minutes}m";
    }

    public static string Humanise(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
}
