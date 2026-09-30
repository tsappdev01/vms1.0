using System.Text.RegularExpressions;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// The fifteen-digit number on the front of the card, and the arithmetic that says whether
/// it was read correctly.
///
/// This exists so the front of a card is worth photographing. The machine-readable zone is
/// only on the back, so for a while the answer to "which side?" had to be "the back, and it
/// matters" - which is a thing to explain to every officer, and to every visitor holding a
/// phone the wrong way round.
///
/// The number carries its own check digit: the fifteenth is a Luhn checksum over the first
/// fourteen. Verified against every Emirates ID this project has seen - the sample card and
/// ten in the live visitor report - all fifteen digits, all passing, and a single altered
/// digit failing. So a number read off the front can be checked as firmly as the zone's, and
/// either side of the card now leads somewhere.
///
/// What the front cannot give is the rest: the name, the dates and the nationality are
/// printed but unchecked, so they are read as a suggestion and the officer confirms them.
/// The back remains the better side, and the screen still says so.
/// </summary>
public static partial class EmiratesIdNumber
{
    public const int Digits = 15;

    /// <summary>Every Emirates ID begins 784 - the UAE's ISO 3166 numeric code.</summary>
    public const string Prefix = "784";

    [GeneratedRegex(@"784[\D]{0,3}(?:\d[\D]{0,3}){12}")]
    private static partial Regex Candidate();

    /// <summary>
    /// True when the number is fifteen digits beginning 784 whose Luhn check digit holds.
    /// </summary>
    public static bool IsValid(string? number)
    {
        if (number is null || number.Length != Digits) return false;
        if (!number.StartsWith(Prefix, StringComparison.Ordinal)) return false;
        if (!number.All(char.IsAsciiDigit)) return false;

        return Luhn(number);
    }

    /// <summary>
    /// The first valid Emirates ID number in a block of recognised text, or null.
    ///
    /// The front prints it as 784-1980-5919869-1, and a recogniser will return those
    /// hyphens, or spaces, or both, or neither. So the candidate is found loosely and judged
    /// strictly: anything non-digit between the digits is ignored, and the fifteen digits
    /// that fall out either satisfy Luhn or are not an Emirates ID number.
    /// </summary>
    public static string? FindIn(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        foreach (Match match in Candidate().Matches(text.ToUpperInvariant()))
        {
            var digits = new string(match.Value.Where(char.IsAsciiDigit).ToArray());

            /* The regex can over-reach into a number that follows, so the first fifteen
               digits are taken rather than all of them. */
            if (digits.Length < Digits) continue;

            var candidate = digits[..Digits];
            if (IsValid(candidate)) return candidate;
        }

        return null;
    }

    /// <summary>
    /// Luhn, right to left, doubling every second digit and casting out nines.
    ///
    /// The same checksum a payment card uses. It catches every single-digit error and almost
    /// every transposition of two adjacent digits, which between them are what OCR does to a
    /// row of figures.
    /// </summary>
    private static bool Luhn(string number)
    {
        var total = 0;
        var doubling = false;

        for (var i = number.Length - 1; i >= 0; i--)
        {
            var digit = number[i] - '0';

            if (doubling)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            total += digit;
            doubling = !doubling;
        }

        return total % 10 == 0;
    }
}
