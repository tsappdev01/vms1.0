using System.Text.RegularExpressions;
using DI.Vms.Domain.Common;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Domain.ValueObjects;

/// <summary>
/// A government ID number together with the masked form shown to users who lack the
/// permission to see it in full (BRD 22).
/// </summary>
public sealed partial class IdNumber : IEquatable<IdNumber>
{
    private IdNumber(string value, IdType idType)
    {
        Value = value;
        IdType = idType;
    }

    /// <summary>The full number. Never serialise this to a client without an explicit permission check.</summary>
    public string Value { get; }

    public IdType IdType { get; }

    /// <summary>
    /// The form normal Security users see, e.g. <c>784-XXXX-XXXXXXX-X</c> for an Emirates ID.
    /// </summary>
    public string Masked => Mask(Value, IdType);

    public static IdNumber Create(string value, IdType idType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("ID number is required.");
        }

        var normalised = Normalise(value, idType);

        if (idType == IdType.EmiratesId && !EmiratesIdPattern().IsMatch(normalised))
        {
            throw new DomainException(
                "An Emirates ID number must be 15 digits, optionally grouped as 784-YYYY-NNNNNNN-C.");
        }

        return new IdNumber(normalised, idType);
    }

    /// <summary>
    /// Canonical form used for storage, comparison and hashing. Separators are stripped from
    /// Emirates IDs so that 784-1985-1234567-1 and 784198512345671 are the same visitor -
    /// otherwise repeat-visitor lookup would miss and the unique index would admit duplicates.
    /// </summary>
    public static string Normalise(string value, IdType idType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim().ToUpperInvariant();

        return idType == IdType.EmiratesId
            ? new string(trimmed.Where(char.IsDigit).ToArray())
            : trimmed.Replace(" ", string.Empty).Replace("-", string.Empty);
    }

    /// <summary>
    /// Masks a number for display. Emirates IDs keep their 784 issuer prefix and check digit,
    /// everything else keeps only the last four characters.
    /// </summary>
    public static string Mask(string value, IdType idType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (idType == IdType.EmiratesId)
        {
            var digits = new string(value.Where(char.IsDigit).ToArray());
            return digits.Length == 15
                ? $"{digits[..3]}-XXXX-XXXXXXX-{digits[^1]}"
                : "XXXXXXXXXXXXXXX";
        }

        return value.Length <= 4
            ? new string('X', value.Length)
            : new string('X', value.Length - 4) + value[^4..];
    }

    public bool Equals(IdNumber? other) =>
        other is not null && IdType == other.IdType && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as IdNumber);

    public override int GetHashCode() => HashCode.Combine(Value, IdType);

    /// <summary>Renders the masked form, so an accidental interpolation never leaks the number.</summary>
    public override string ToString() => Masked;

    [GeneratedRegex(@"^784[- ]?\d{4}[- ]?\d{7}[- ]?\d$|^\d{15}$")]
    private static partial Regex EmiratesIdPattern();
}
