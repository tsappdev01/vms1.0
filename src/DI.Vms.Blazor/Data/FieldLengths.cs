namespace DI.Vms.Blazor.Data;

/// <summary>
/// The column widths, in one place.
///
/// Both the model configuration and the screens need these, and two copies drift: a form
/// that lets 400 characters be typed into a 300-character column fails at the database
/// with a message about truncation, at the desk, with a visitor waiting.
/// </summary>
public static class FieldLengths
{
    public const int IdNumber = 30;
    public const int CardNumber = 30;
    public const int Name = 300;
    public const int PersonToVisit = 200;
    public const int Purpose = 60;
    public const int PurposeOther = 200;
    public const int Title = 200;
    public const int Email = 256;
    public const int Company = 200;
    public const int CaptureMethod = 30;

    /// <summary>Everything read off the card that is not a name.</summary>
    public const int CardField = 150;

    /// <summary>An Emirates ID number: 15 digits, the first three being 784.</summary>
    public const int EmiratesIdDigits = 15;

    /// <summary>
    /// Cuts a value to what the column will hold.
    ///
    /// Nothing a real card returns comes near these, so a clamp here is not routine
    /// tidying - it is the boundary that stops a response the server could not verify
    /// from turning into a database error instead of a record. Returns whether it cut, so
    /// the caller can say so rather than truncating in silence.
    /// </summary>
    public static string? Clamp(string? value, int maximum, out bool cut)
    {
        cut = value is not null && value.Length > maximum;
        return cut ? value![..maximum] : value;
    }

    /// <summary>As above, where the caller has no use for knowing.</summary>
    public static string? Clamp(string? value, int maximum) => Clamp(value, maximum, out _);
}
