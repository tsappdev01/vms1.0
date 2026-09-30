using System.Globalization;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// What a person typed, and whether it can be believed.
///
/// One definition, because there are three places that ask - the desk browser, the tablet
/// and the API - and until this existed they disagreed. The web required a mobile number
/// and the tablet called it optional; the web warned that an ID number was twelve digits
/// and saved it anyway; the server asked only whether the ID number contained a digit. So
/// "7" was a valid Emirates ID, and a mobile number could be the word "abc".
///
/// The split between the two kinds of answer is the point of the class:
///
/// <b>A problem blocks.</b> The ID number is the one field a repeat visit is matched on, so
/// a wrong one does not produce a bad record - it produces a second person. It carries its
/// own check digit, the officer is holding the card, and retyping it costs seconds.
///
/// <b>A concern does not.</b> A mobile number in an unusual shape and a date the parser does
/// not recognise are worth saying out loud and never worth refusing a visitor over. Reception
/// has somebody standing in front of them, and a desk that will not proceed is a desk that
/// writes the visit on paper.
///
/// The tablet keeps its own copy of the cheap parts of this - it cannot call C# - and the
/// server is what actually decides. That is the same arrangement the card reading uses, for
/// the same reason: the rule that matters must live where the record is written.
/// </summary>
public static class VisitorFields
{
    /// <summary>Just the digits, which is what is stored and what the rules are about.</summary>
    public static string Digits(string? raw) =>
        raw is null ? string.Empty : new string(raw.Where(char.IsAsciiDigit).ToArray());

    /// <summary>
    /// Why a typed Emirates ID number cannot be accepted, or null.
    ///
    /// Blocking, and only for a number somebody typed. A chip read carries a signed document
    /// and a photographed card has already passed this same check digit in
    /// <see cref="EmiratesIdNumber"/> - neither is second-guessed here, because the card is
    /// the authority on what is printed on the card.
    /// </summary>
    public static string? TypedIdNumberProblem(string? typed)
    {
        var digits = Digits(typed);

        if (digits.Length == 0) return "An ID number is required.";

        if (digits.Length != EmiratesIdNumber.Digits)
        {
            return $"An Emirates ID number is {EmiratesIdNumber.Digits} digits. " +
                $"That is {digits.Length}.";
        }

        if (!digits.StartsWith(EmiratesIdNumber.Prefix, StringComparison.Ordinal))
        {
            return $"An Emirates ID number begins {EmiratesIdNumber.Prefix}.";
        }

        /* The fifteenth digit is a Luhn checksum over the first fourteen, so a single wrong
           digit and almost every swap of two adjacent ones is caught here rather than in the
           report six months later. Verified against every card this project has seen. */
        if (!EmiratesIdNumber.IsValid(digits))
        {
            return "That is fifteen digits but not a valid Emirates ID number - " +
                "check it against the card.";
        }

        return null;
    }

    /// <summary>
    /// What is odd about a telephone number, or null. Never blocking.
    ///
    /// Deliberately generous. Visitors give landlines, foreign numbers and numbers with
    /// extensions, and a reception desk is the wrong place to argue about any of them. What
    /// this catches is the number that is not a number at all, and the UAE mobile that is a
    /// digit short - which is the mistake that actually happens, and which nothing would
    /// otherwise notice until somebody tried to ring it.
    /// </summary>
    public static string? MobileConcern(string? typed)
    {
        /* Blank is not this rule's business. Whether the field is required is a separate
           question, asked in one place, and answering it here too would say it twice. */
        if (string.IsNullOrWhiteSpace(typed)) return null;

        if (typed.Any(char.IsLetter))
        {
            return "That has letters in it. A telephone number is digits, with + and spaces " +
                "if they help.";
        }

        var digits = Digits(typed);

        if (digits.Length < 7) return "That looks too short for a telephone number.";

        /* E.164 caps a subscriber number at fifteen digits, country code included. */
        if (digits.Length > 15) return "That is longer than any telephone number.";

        var national = digits;
        if (national.StartsWith("00", StringComparison.Ordinal)) national = national[2..];
        if (national.StartsWith("971", StringComparison.Ordinal)) national = national[3..];
        national = national.TrimStart('0');

        /* UAE mobiles are the 5 series and are nine digits nationally. Landlines and foreign
           numbers fall out of this test rather than into it, which is intended. */
        if (national.StartsWith("5", StringComparison.Ordinal) && national.Length != 9)
        {
            return "A UAE mobile is nine digits after the country code - 05x xxx xxxx.";
        }

        return null;
    }

    /// <summary>
    /// The date formats a card field has been seen in.
    ///
    /// The zone produces dd/MM/yyyy, because <see cref="MachineReadableZone"/> writes it that
    /// way. What the chip's XML produces is not documented in the toolkit reference and this
    /// project has no sample to read, so the list is wide and the answer is a concern rather
    /// than a refusal: an unrecognised format must never be able to stop a check-in.
    /// </summary>
    private static readonly string[] CardDateFormats =
    [
        "dd/MM/yyyy", "d/M/yyyy",
        "dd-MM-yyyy", "d-M-yyyy",
        "dd.MM.yyyy",
        "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd",
        "dd MMM yyyy", "d MMM yyyy",
    ];

    /// <summary>A card date as a date, or null when it is in none of the shapes above.</summary>
    public static DateOnly? ReadCardDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        foreach (var format in CardDateFormats)
        {
            if (DateOnly.TryParseExact(
                    value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
            {
                return date;
            }
        }

        return null;
    }

    /// <summary>
    /// What is odd about a card expiry date, or null. Never blocking.
    ///
    /// Two things are worth saying: that the date will not be readable as a date, and that
    /// the card has already expired. The second is the one reception would want to know and
    /// the one nothing anywhere said before - an expired Emirates ID was recorded exactly as
    /// a current one.
    /// </summary>
    public static string? ExpiryConcern(string? value, DateOnly? today = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var date = ReadCardDate(value);
        if (date is null)
        {
            return "That is not a date the report will be able to read. " +
                "The card prints it as 25/08/2028.";
        }

        if (date < (today ?? Today))
        {
            return $"That card expired on {date:dd MMM yyyy}.";
        }

        return null;
    }

    /// <summary>
    /// Today at the desk, which is in Dubai.
    ///
    /// UTC+4 with no daylight saving, so a fixed offset is the whole of it and no time zone
    /// database is needed on a Linux App Service. Four hours matters here: a card expiring
    /// today reads as expired for the first four hours of the working day if this is UTC.
    /// </summary>
    public static DateOnly Today =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(4));
}
