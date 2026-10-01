using System.Text.RegularExpressions;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// The fields printed in plain sight on a card that has no machine-readable zone.
///
/// It exists because of the card nobody at this desk is holding: the UAE Pass digital ID on a
/// phone. It prints the ID number, the English and Arabic names, the issue and expiry dates
/// and the nationality - and carries a QR code instead of a zone, so
/// <see cref="MachineReadableZone"/> finds nothing and the visitor is left with the ID number
/// alone while their name and expiry date sit on the screen being ignored. The front of the
/// plastic card is the same problem in smaller type.
///
/// <b>Nothing here is checked by arithmetic, and that is the whole difference.</b> The zone
/// carries a check digit on every field, and the fifteen-digit number carries a Luhn digit,
/// so both can be believed on their own. A printed name has nothing of the kind: it is read
/// the way a person reads it, with all the mistakes that implies. So what comes out of this
/// is a <b>suggestion for the officer to confirm</b>, never a verified read, and a visit made
/// from it is recorded as Manual - the desk saw the card, the system did not.
///
/// Two things it will not do:
///
/// <b>It will not pair a label with a value by position.</b> A recogniser returns lines in
/// reading order, and the UAE Pass card puts three labels across one row and their three
/// values across the next; the plastic card puts the label and the value on the same line
/// sometimes and on consecutive lines otherwise. Matching by position would be right on
/// whichever layout it was written against and quietly wrong on the other - and quietly wrong
/// here means a date of birth recorded as an expiry date. So dates are taken by what they are
/// rather than by where they sit: of the dates on an Emirates ID the earliest is the birth and
/// the latest is the expiry, which holds whatever order they were read in.
///
/// <b>It will not read the QR code.</b> On the UAE Pass card that code is a verification token
/// for ICP's own service, not the card's contents, so there is nothing in it to extract.
/// </summary>
public static partial class PrintedCard
{
    /// <summary>
    /// What could be read off the face of the card. Every one of them is a suggestion.
    /// </summary>
    /// <param name="NameWasCut">
    /// The app itself truncated the name with an ellipsis, as UAE Pass does when it is too
    /// long for the box - "SENTHIL KUMAR PONNUSAMY SAKTHIVEL…". What was read is correct as
    /// far as it goes and is not the whole name, which is a different thing from a misread
    /// and has to be said differently: the officer must finish it rather than check it.
    /// </param>
    public sealed record Fields(
        string? FullNameEnglish,
        bool NameWasCut,
        string? ExpiryDate,
        string? DateOfBirth,
        string? NationalityEnglish,
        string? CardNumber)
    {
        /// <summary>Whether anything at all was found worth offering.</summary>
        public bool Any =>
            FullNameEnglish is not null || ExpiryDate is not null ||
            DateOfBirth is not null || NationalityEnglish is not null || CardNumber is not null;
    }

    public static readonly Fields Nothing = new(null, false, null, null, null, null);

    public static Fields Read(string? recognised)
    {
        if (string.IsNullOrWhiteSpace(recognised)) return Nothing;

        var lines = recognised
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var (name, cut) = Name(lines);
        var (expiry, born) = Dates(lines);

        return new Fields(
            FullNameEnglish: name,
            NameWasCut: cut,
            ExpiryDate: expiry,
            DateOfBirth: born,
            NationalityEnglish: Nationality(lines),
            CardNumber: CardNumber(lines));
    }

    // ------------------------------------------------------------------ dates

    /// <summary>
    /// The expiry and the birth date, told apart by what they are.
    ///
    /// An Emirates ID prints two or three dates: born, issued and expires. Whatever order a
    /// recogniser returns them in, the earliest is the birth and the latest is the expiry -
    /// a card is issued after its holder is born and expires after it is issued, and no
    /// layout changes that. A label on the same line is better evidence still and is used
    /// when there is one, but the ordering is what makes this work on a layout nobody has
    /// shown me.
    ///
    /// With one date and no label, nothing is returned. It could be either, and a date of
    /// birth written into the expiry field is worse than an empty expiry field: the empty one
    /// stops the save and gets asked about.
    /// </summary>
    private static (string? Expiry, string? Born) Dates(List<string> lines)
    {
        string? labelledExpiry = null;
        string? labelledBirth = null;
        var all = new List<(DateOnly Date, string Text)>();

        foreach (var line in lines)
        {
            foreach (Match match in DateLike().Matches(line))
            {
                var parsed = VisitorFields.ReadCardDate(match.Value);
                if (parsed is null) continue;

                all.Add((parsed.Value, match.Value));

                /* A label beside the value is the one case where position says something
                   trustworthy, because the two were on the same line in the source. */
                if (ExpiryLabel().IsMatch(line)) labelledExpiry ??= match.Value;
                if (BirthLabel().IsMatch(line)) labelledBirth ??= match.Value;
            }
        }

        if (all.Count == 0) return (null, null);

        var expiry = labelledExpiry ?? (all.Count >= 2 ? all.MaxBy(d => d.Date).Text : null);
        var born = labelledBirth;

        if (born is null && all.Count >= 2)
        {
            /*  The earliest date, but only if it could be a birth date at all.
             *
             *  This is the correction that matters. The UAE Pass card prints an issue date and
             *  an expiry date and no birth date, so "the earliest of them" is the issue date -
             *  and taking it would write the day the card was printed into the visitor's date
             *  of birth. Plausible on the plastic card, where a birth date really is the
             *  earliest of three, and wrong on the digital one, where there are only two.
             *
             *  So the earliest date has to look like a birth as well as be the earliest. An
             *  Emirates ID is valid for at most ten years, which puts every issue date inside
             *  that window and every adult visitor's birth outside it. A visitor young enough
             *  to fail this is a visitor young enough that reception is typing it in anyway. */
            var earliest = all.MinBy(d => d.Date);
            if (earliest.Date < VisitorFields.Today.AddYears(-OldestPlausibleCard)) born = earliest.Text;
        }

        /* Both ends of one date is not two dates. */
        if (expiry is not null && expiry == born) { expiry = null; born = null; }

        return (expiry, born);
    }

    /// <summary>
    /// The longest an Emirates ID is issued for. Nothing printed on a valid card was issued
    /// before this, so a date older than it is not an issue date.
    /// </summary>
    private const int OldestPlausibleCard = 10;

    // ------------------------------------------------------------------ name

    private static (string? Name, bool Cut) Name(List<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            /*  Anchored, which is what keeps the Arabic label out.
             *
             *  "ARABIC NAME" contains "NAME", so an unanchored search finds it. Matching only
             *  at the start of the line excludes it without a second test - and a second test
             *  was worse than nothing: a layout that puts both captions on one row, as
             *  "ENGLISH NAME: ARABIC NAME:", had the whole line discarded and the name below
             *  it never looked at. */
            if (!NameLabel().IsMatch(line)) continue;

            /* The value sits after the label on the same line - "Name: Senthil Kumar" - or on
               the line below it, which is what the UAE Pass card does. Both are tried here
               rather than assumed, because a card prints whichever suits its layout. */
            var sameLine = Usable(NameLabel().Replace(line, string.Empty, 1));
            if (sameLine is not null) return Trimmed(sameLine);

            if (i + 1 < lines.Count)
            {
                var below = Usable(lines[i + 1]);
                if (below is not null) return Trimmed(below);
            }
        }

        return (null, false);
    }

    /// <summary>
    /// A value, or null when the line is punctuation, another label, or has digits in it.
    ///
    /// Digits disqualify it because every other field on these cards is a number or a date
    /// and a name is not - so a line with a digit in it is some other field that the layout
    /// happened to put where a name was expected.
    /// </summary>
    private static string? Usable(string raw)
    {
        var text = raw.Trim().TrimStart(':', '/', '-').Trim();

        if (text.Length < 2) return null;
        if (text.Any(char.IsAsciiDigit)) return null;
        if (!text.Any(char.IsLetter)) return null;
        if (AnyLabel().IsMatch(text)) return null;

        return text;
    }

    /// <summary>Strips the ellipsis UAE Pass adds, and says that it was there.</summary>
    private static (string? Name, bool Cut) Trimmed(string value)
    {
        var cut = value.EndsWith('…') || value.EndsWith("...", StringComparison.Ordinal);
        var name = value.TrimEnd('…', '.', ' ').Trim();

        return name.Length < 2 ? (null, false) : (name, cut);
    }

    // ------------------------------------------------------------------ the rest

    private static string? Nationality(List<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (!NationalityLabel().IsMatch(lines[i])) continue;

            var sameLine = Usable(NationalityLabel().Replace(lines[i], string.Empty, 1));
            if (sameLine is not null) return sameLine;

            if (i + 1 < lines.Count)
            {
                var below = Usable(lines[i + 1]);
                if (below is not null) return below;
            }
        }

        return null;
    }

    /// <summary>
    /// The card number, which is printed on the back of the plastic card and nowhere on the
    /// digital one. Nine digits, and deliberately not taken from a line that also holds the
    /// fifteen-digit ID number - the two are easy to confuse and only one of them is checked.
    /// </summary>
    private static string? CardNumber(List<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (!CardNumberLabel().IsMatch(lines[i])) continue;

            foreach (var candidate in new[] { lines[i], i + 1 < lines.Count ? lines[i + 1] : "" })
            {
                var digits = VisitorFields.Digits(candidate);
                if (digits.Length is >= 8 and <= 10) return digits;
            }
        }

        return null;
    }

    // ------------------------------------------------------------------ labels

    [GeneratedRegex(@"\d{1,4}\s*[/\-.]\s*\d{1,2}\s*[/\-.]\s*\d{1,4}")]
    private static partial Regex DateLike();

    [GeneratedRegex(@"EXPIR\w*\s*(DATE)?", RegexOptions.IgnoreCase)]
    private static partial Regex ExpiryLabel();

    [GeneratedRegex(@"(DATE\s*OF\s*BIRTH|BIRTH\s*DATE|\bDOB\b)", RegexOptions.IgnoreCase)]
    private static partial Regex BirthLabel();

    [GeneratedRegex(@"^\s*(ENGLISH\s+)?NAME\s*[:/]?", RegexOptions.IgnoreCase)]
    private static partial Regex NameLabel();

    [GeneratedRegex(@"^\s*NATIONALITY\s*[:/]?", RegexOptions.IgnoreCase)]
    private static partial Regex NationalityLabel();

    [GeneratedRegex(@"CARD\s*NUMBER", RegexOptions.IgnoreCase)]
    private static partial Regex CardNumberLabel();

    /// <summary>
    /// Anything that is a caption rather than a value.
    ///
    /// Without this the line below "ENGLISH NAME:" on a layout that puts the Arabic caption
    /// there is read as the visitor's name.
    /// </summary>
    [GeneratedRegex(
        @"^(ID\s*NUMBER|CARD\s*NUMBER|ENGLISH\s*NAME|ARABIC\s*NAME|NAME|NATIONALITY|SEX|GENDER|"
        + @"DATE\s*OF\s*BIRTH|ISSUE\s*DATE|ISSUING\s*DATE|EXPIRE\s*DATE|EXPIRY\s*DATE|"
        + @"IDENTITY\s*TYPE|RESIDENT|CITIZEN|SIGNATURE|OCCUPATION|EMPLOYER|ISSUING\s*PLACE|"
        + @"UNITED\s*ARAB\s*EMIRATES|RESIDENT\s*IDENTITY\s*CARD|FEDERAL\s*AUTHORITY.*)\s*[:/]?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex AnyLabel();
}
