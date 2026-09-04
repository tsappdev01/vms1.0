namespace DI.Vms.Blazor.Services;

/// <summary>Everything the screen shows, as the chip presented it.</summary>
public sealed class CardData
{
    public string IdNumber { get; set; } = string.Empty;
    public string? CardNumber { get; set; }
    public byte[]? Photo { get; set; }

    public string? IdType { get; set; }
    public string? IssueDate { get; set; }
    public string? ExpiryDate { get; set; }

    /// <summary>The chip's comma-delimited segments joined into a readable name.</summary>
    public string FullNameEnglish { get; set; } = string.Empty;

    /// <summary>The raw value, kept because the segments carry name positions.</summary>
    public string? FullNameRaw { get; set; }

    public string? FullNameArabic { get; set; }
    public string? TitleEnglish { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalityEnglish { get; set; }

    /// <summary>Read for the card facsimile, which prints nationality in both scripts.</summary>
    public string? NationalityArabic { get; set; }

    public string? NationalityCode { get; set; }
    public string? PlaceOfBirthEnglish { get; set; }

    public string? AddressEmirate { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressArea { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressBuilding { get; set; }
    public string? AddressPoBox { get; set; }
    public string? AddressPhone { get; set; }
    public string? AddressMobile { get; set; }
    public string? AddressEmail { get; set; }

    /// <summary>Set when the response signature could not be verified. Not fatal, but shown.</summary>
    public string? SignatureWarning { get; set; }

    /// <summary>
    /// True when the chip carried no postal address. Observed on the card tested: every
    /// address field blank except mobile and email. The screen must say so rather than
    /// render empty boxes that look like a failed read.
    /// </summary>
    public bool HasNoPostalAddress =>
        string.IsNullOrWhiteSpace(AddressEmirate) && string.IsNullOrWhiteSpace(AddressCity) &&
        string.IsNullOrWhiteSpace(AddressArea) && string.IsNullOrWhiteSpace(AddressStreet) &&
        string.IsNullOrWhiteSpace(AddressBuilding) && string.IsNullOrWhiteSpace(AddressPoBox);
}

public sealed class ReaderState
{
    public bool Available { get; init; }
    public string? ReaderName { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string? ToolkitVersion { get; init; }

    /// <summary>As the toolkit reports it, which is a string of its own choosing.</summary>
    public string? LicenceExpiry { get; init; }

    /// <summary>
    /// Days until the licence expires, or null when the toolkit's date could not be
    /// parsed. Worth surfacing because an expired licence stops every read, and the last
    /// one took a fortnight to replace.
    /// </summary>
    public int? LicenceDaysRemaining { get; init; }
}
