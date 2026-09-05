namespace DI.Vms.Blazor.Data;

/// <summary>A company within the Dubai Investments group.</summary>
public class DiEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Someone a visitor can come to see, as exported from the address list: display name,
/// title, email and company.
/// </summary>
public class Person
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }

    /// <summary>The company as the address list spells it, e.g. "Dubai Investments Park".</summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// The entity this person belongs to, where the company could be matched to one.
    /// Null when it could not - the address list's company names and the entity list do
    /// not fully agree - and those people are then found only by searching all entities.
    /// </summary>
    public int? DiEntityId { get; set; }
    public DiEntity? DiEntity { get; set; }

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
    /// <summary>
    /// The cardholder's signature image, as stored by earlier reads.
    ///
    /// No longer read or written: the chip's format never rendered in a browser and the
    /// slot showed a broken image, so the read stopped asking for it. The column stays so
    /// the rows that already carry one remain readable, and so removing it does not need
    /// a migration. To bring it back, ask for it again in CardReaderService and
    /// card-agent.js - both pass false for the signature image now.
    /// </summary>
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
    /// <summary>
    /// The host's name as recorded - the display name of whoever was picked, or exactly
    /// what was typed when the host was not in the list.
    /// </summary>
    public required string PersonToVisit { get; set; }

    /// <summary>
    /// The person picked from the list, when one was. Null for a typed name, which is
    /// what tells the two apart.
    /// </summary>
    public int? PersonToVisitId { get; set; }
    public Person? PersonToVisitPerson { get; set; }

    /* Copied from the person at the time of the visit rather than joined at read time.
       A host who changes title, or leaves, must not silently rewrite what last year's
       report said - the same reason the purpose stores its label and not an id. */
    public string? PersonToVisitTitle { get; set; }
    public string? PersonToVisitEmail { get; set; }
    public string? PersonToVisitCompany { get; set; }

    /// <summary>
    /// Why they are here, from the fixed list the desk offers. Stored as the chosen
    /// label rather than an id, so a report over last year still reads correctly if the
    /// list is later changed.
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// What was typed when the purpose was "Other". Null for every other purpose - the
    /// two are kept apart so the report can still group by Purpose.
    /// </summary>
    public string? PurposeOther { get; set; }

    /// <summary>The date-time stamp of the entry, in UTC. Displayed as Gulf Standard Time.</summary>
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>How the identity reached the system, so manual entries are distinguishable.</summary>
    public string CaptureMethod { get; set; } = "CardReader";

    /// <summary>
    /// Who saved this entry, as Entra ID knows them.
    ///
    /// Null on everything recorded before sign-in existed, and null is not "unknown user"
    /// - it is "recorded when the system had no idea who anyone was". Worth keeping the
    /// distinction rather than backfilling something that reads like a name.
    /// </summary>
    public string? RecordedBy { get; set; }
}
