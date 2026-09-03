namespace DI.Vms.Blazor.Data;

/// <summary>A company within the Dubai Investments group.</summary>
public class DiEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// One visitor entry. Card fields are stored as the chip presents them, so the record
/// reflects what was actually read rather than an interpretation of it.
/// </summary>
public class VisitorEntry
{
    public int Id { get; set; }

    // ---- Identity
    public required string IdNumber { get; set; }
    public string? CardNumber { get; set; }

    /// <summary>JPEG from the chip.</summary>
    public byte[]? Photo { get; set; }

    /// <summary>The holder's signature as held on the card.</summary>
    public byte[]? CardSignature { get; set; }

    // ---- Non-modifiable data
    public string? IdType { get; set; }
    public string? IssueDate { get; set; }
    public string? ExpiryDate { get; set; }

    /// <summary>Joined from the chip's comma-delimited segments, e.g. "NAYYAR JAWAID ALI KHAN".</summary>
    public required string FullNameEnglish { get; set; }

    /// <summary>The raw value, kept because the segments carry given/middle/family positions.</summary>
    public string? FullNameRaw { get; set; }

    public string? FullNameArabic { get; set; }
    public string? TitleEnglish { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? NationalityEnglish { get; set; }
    public string? NationalityCode { get; set; }
    public string? PlaceOfBirthEnglish { get; set; }

    // ---- Home address. Every field except mobile and email was empty on the card
    // tested, so nothing here may be assumed present.
    public string? AddressEmirate { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressArea { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressBuilding { get; set; }
    public string? AddressPoBox { get; set; }
    public string? AddressPhone { get; set; }
    public string? AddressMobile { get; set; }
    public string? AddressEmail { get; set; }

    // ---- Visit
    public int DiEntityId { get; set; }
    public DiEntity? DiEntity { get; set; }
    public required string PersonToVisit { get; set; }

    /// <summary>The date-time stamp of the entry, in UTC. Displayed as Gulf Standard Time.</summary>
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>How the identity reached the system, so manual entries are distinguishable.</summary>
    public string CaptureMethod { get; set; } = "CardReader";
}
