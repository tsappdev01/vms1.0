using System.Text;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// The three lines printed on the back of an Emirates ID, parsed and checked.
///
/// This is ICAO 9303 TD1: three lines of thirty characters in OCR-B, the same zone a
/// passport gate reads. It exists here because a visitor increasingly arrives with the card
/// on a phone rather than in a wallet, and a photograph of the front would have to be read
/// by OCR with nothing to say whether it was read correctly.
///
/// The MRZ has check digits. That is the whole reason to prefer it: a camera pointed at a
/// phone screen through glare and moiré will misread characters, and arithmetic over the
/// result turns a wrong ID number into a refusal instead of into a visitor record. Every
/// field this returns has survived its own check digit and the composite over all of them.
///
/// What it cannot do is tell you the card is real. The MRZ is printed, not signed - anyone
/// can produce an image carrying a well-formed one. A read through this path is recorded as
/// <c>DigitalCard</c> for exactly that reason, below an unverified chip read rather than
/// beside a verified one.
/// </summary>
public static class MachineReadableZone
{
    /// <summary>What a TD1 line is: three of them, thirty characters each.</summary>
    public const int LineLength = 30;
    public const int LineCount = 3;

    /// <summary>
    /// The characters the zone is allowed to contain. Anything else is a misread, and
    /// saying so beats letting it reach a check digit as a zero.
    /// </summary>
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789<";

    public sealed record Result(
        bool Ok,
        string? Problem,
        string? IdNumber,
        string? CardNumber,
        string? FullNameEnglish,
        string? DateOfBirth,
        string? ExpiryDate,
        string? Gender,
        string? NationalityCode);

    private static Result Failed(string problem) =>
        new(false, problem, null, null, null, null, null, null, null);

    /// <summary>
    /// Tidies what OCR returned into three lines of thirty, or says why it could not.
    ///
    /// Separate from parsing because the cleanup is guesswork and the parsing is not:
    /// spaces the recogniser invented are dropped, and the common OCR-B confusions are
    /// left alone rather than corrected, because "fixing" O to 0 in a name is how a
    /// surname quietly changes.
    /// </summary>
    public static string[]? Normalise(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var lines = raw
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => new string(line.ToUpperInvariant().Where(c => Alphabet.Contains(c)).ToArray()))
            .Where(line => line.Length > 0)
            .ToArray();

        /* A recogniser sometimes returns the zone as one run with no line breaks. Ninety
           characters split evenly is that case, and it is unambiguous. */
        if (lines.Length == 1 && lines[0].Length == LineLength * LineCount)
        {
            var one = lines[0];
            lines = [one[..30], one[30..60], one[60..]];
        }

        return lines.Length == LineCount ? lines : null;
    }

    /// <summary>
    /// Every position's type, because OCR-B confuses characters that the standard never
    /// allows to be confused.
    ///
    /// <c>D</c> a digit, <c>A</c> a letter or filler, <c>*</c> either. A recogniser reading a
    /// photograph will offer 1 for I and 0 for O all day; the zone says which of the two a
    /// given position can possibly be, so the reading can be corrected rather than refused -
    /// and the check digits still have the last word on whether the correction was right.
    /// </summary>
    /* Positions 16-30 are "optional data" in the standard and could be anything. On an
       Emirates ID they are the fifteen-digit ID number, which is the field this whole
       exercise exists to read - so they are digits here, and an O read for a 0 in the middle
       of it is corrected rather than refused. */
    private const string Line1Shape = "AAAAA*********DDDDDDDDDDDDDDDD";
    private const string Line2Shape = "DDDDDDDADDDDDDDAAA***********D";
    private const string Line3Shape = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    /// <summary>The shapes a recogniser mistakes for a digit, and what it meant.</summary>
    private static readonly Dictionary<char, char> AsDigit = new()
    {
        ['O'] = '0', ['Q'] = '0', ['D'] = '0', ['U'] = '0',
        ['I'] = '1', ['L'] = '1',
        ['Z'] = '2', ['A'] = '4', ['S'] = '5', ['G'] = '6', ['T'] = '7', ['B'] = '8',
    };

    private static readonly Dictionary<char, char> AsLetter = new()
    {
        ['0'] = 'O', ['1'] = 'I', ['2'] = 'Z', ['4'] = 'A',
        ['5'] = 'S', ['6'] = 'G', ['7'] = 'T', ['8'] = 'B',
    };

    /// <summary>
    /// Puts a line back to the shape the standard says it has.
    ///
    /// Only substitutions the recogniser is known to make, and only where the position
    /// allows exactly one kind of character. A letter in a field that holds a name is left
    /// alone, because there is nothing to correct it against - which is why line three, the
    /// one with no check digit, is also the one this cannot rescue.
    /// </summary>
    private static string Reshape(string line, string shape)
    {
        var fixedUp = line.ToCharArray();

        for (var i = 0; i < fixedUp.Length && i < shape.Length; i++)
        {
            var c = fixedUp[i];

            if (shape[i] == 'D' && !char.IsDigit(c) && AsDigit.TryGetValue(c, out var digit))
            {
                fixedUp[i] = digit;
            }
            else if (shape[i] == 'A' && char.IsDigit(c) && AsLetter.TryGetValue(c, out var letter))
            {
                fixedUp[i] = letter;
            }
        }

        return new string(fixedUp);
    }

    /// <summary>
    /// Pads a line that is short only in its run of filler.
    ///
    /// A chevron is the easiest character in the zone to lose - it is thin, and a run of
    /// eleven of them photographs as a dashed line. Losing one shortens the line without
    /// touching a single field, and the composite check digit still proves whether putting
    /// it back was right.
    /// </summary>
    private static string PadFiller(string line)
    {
        if (line.Length >= LineLength) return line;

        var run = 0;
        var at = -1;

        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '<')
            {
                if (run == 0) at = i;
                run++;
            }
            else if (run > 0)
            {
                if (run >= 3) break;
                run = 0;
                at = -1;
            }
        }

        // Only a run long enough to be filler rather than a name separator.
        if (at < 0 || run < 3) return line;

        return line.Insert(at, new string('<', LineLength - line.Length));
    }

    public static Result Parse(string? raw)
    {
        var lines = Normalise(raw);

        if (lines is null)
        {
            return Failed(
                "That does not look like the three lines from the back of the card. " +
                "Photograph the back, with all three lines of letters and chevrons in frame.");
        }

        /* Repair before judging. Each step is reversible by the check digits: if a
           substitution or a pad was wrong, the arithmetic fails and the read is refused,
           exactly as it would have been without them. */
        lines =
        [
            Reshape(PadFiller(lines[0]), Line1Shape),
            Reshape(PadFiller(lines[1]), Line2Shape),
            Reshape(PadFiller(lines[2]), Line3Shape),
        ];

        if (lines.Any(line => line.Length != LineLength))
        {
            var lengths = string.Join(", ", lines.Select(l => l.Length));
            return Failed(
                $"The three lines came out {lengths} characters long; each should be {LineLength}. " +
                "Some characters were missed - try again with the card filling more of the frame.");
        }

        var (one, two, three) = (lines[0], lines[1], lines[2]);

        /* Positions are fixed by the standard, so they are written out rather than hunted
           for. UAE puts the fifteen-digit Emirates ID in line one's optional data, which is
           the field this whole exercise is really after. */
        var cardNumber = one[5..14];
        var emiratesId = one[15..30];
        var dateOfBirth = two[0..6];
        var gender = two[7..8];
        var expiry = two[8..14];
        var nationality = two[15..18];

        var composite = one[5..30] + two[0..7] + two[8..15] + two[18..29];

        foreach (var (name, value, printed) in new[]
                 {
                     ("card number", cardNumber, one[14]),
                     ("date of birth", dateOfBirth, two[6]),
                     ("expiry date", expiry, two[14]),
                     ("whole zone", composite, two[29]),
                 })
        {
            if (CheckDigit(value) != printed)
            {
                /* Deliberately not "invalid card". The card is almost certainly fine; the
                   photograph is what failed, and telling reception to blame the visitor's
                   card would be both wrong and awkward. */
                return Failed(
                    $"The {name} did not add up - the photograph was misread somewhere. " +
                    "Take it again, straighter and with less glare, or enter the details by hand.");
            }
        }

        if (!emiratesId.All(char.IsDigit))
        {
            return Failed(
                "The Emirates ID number came out with letters in it, so the photograph was " +
                "misread. Take it again, or enter the details by hand.");
        }

        return new Result(
            Ok: true,
            Problem: null,
            IdNumber: emiratesId,
            CardNumber: cardNumber.TrimEnd('<'),
            FullNameEnglish: Name(three),
            DateOfBirth: Date(dateOfBirth),
            ExpiryDate: Date(expiry),
            Gender: gender is "M" or "F" ? gender : null,
            NationalityCode: nationality.TrimEnd('<') is { Length: > 0 } code ? code : null);
    }

    /// <summary>
    /// ICAO's check digit: weights of 7, 3 and 1 repeating, letters counted from A = 10.
    /// </summary>
    public static char CheckDigit(string value)
    {
        var weights = new[] { 7, 3, 1 };
        var total = 0;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            var digit = c switch
            {
                '<' => 0,
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => c - 'A' + 10,
                _ => -1,
            };

            // Only reachable when a caller skips Normalise; treated as a failing sum.
            if (digit < 0) return '?';

            total += digit * weights[i % 3];
        }

        return (char)('0' + total % 10);
    }

    /// <summary>
    /// Line three, as a person's name: surname after the double chevron, given names before.
    ///
    /// The zone prints surname first because a machine reads it that way; the desk reads it
    /// the other way round, and the screens already show a given-names-first name from the
    /// chip. So the two agree.
    /// </summary>
    private static string Name(string line)
    {
        var parts = line.Split("<<", 2, StringSplitOptions.None);

        string Words(string s) =>
            string.Join(' ', s.Split('<', StringSplitOptions.RemoveEmptyEntries));

        var surname = Words(parts[0]);
        var given = parts.Length > 1 ? Words(parts[1]) : string.Empty;

        var name = new StringBuilder();
        if (given.Length > 0) name.Append(given);
        if (surname.Length > 0) name.Append(name.Length > 0 ? " " : "").Append(surname);

        return name.ToString();
    }

    /// <summary>
    /// YYMMDD, in the format the rest of the screens use.
    ///
    /// The century is decided the way a reception desk needs it rather than the way the
    /// standard leaves open: a birth date cannot be in the future, and an expiry date on a
    /// card somebody is holding is not fifty years past. Both sit comfortably inside a
    /// window that puts 40 and above in the 1900s.
    /// </summary>
    private static string? Date(string yymmdd)
    {
        if (!yymmdd.All(char.IsDigit)) return null;

        var year = int.Parse(yymmdd[..2]);
        var month = yymmdd[2..4];
        var day = yymmdd[4..6];

        return $"{day}/{month}/{(year > 40 ? 1900 + year : 2000 + year)}";
    }
}
