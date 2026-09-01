using DI.Vms.Domain.Common;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Domain.Entities;

/// <summary>
/// A person, identified once and reused across visits. Repeat-visitor recognition (BRD 14)
/// works by looking up <see cref="IdType"/> + <see cref="IdNumberHash"/>.
/// </summary>
public class Visitor : AuditableEntity
{
    public required string Name { get; set; }
    public string? Company { get; set; }

    public IdType IdType { get; set; }

    /// <summary>
    /// The ID number as ciphertext (BRD 22). Decrypting it requires the permission-gated,
    /// audited path in the application layer - never read it directly for display.
    /// </summary>
    public required byte[] IdNumberCipher { get; set; }

    /// <summary>
    /// Deterministic keyed hash of the normalised ID number, used for lookup and for the
    /// uniqueness index. Lets us find a repeat visitor without decrypting every row.
    /// </summary>
    public required byte[] IdNumberHash { get; set; }

    /// <summary>
    /// The masked form, e.g. <c>784-XXXX-XXXXXXX-1</c>, computed once at registration.
    /// Storing it means listing visitors never decrypts anything: the common read path
    /// touches no plaintext at all.
    /// </summary>
    public required string IdNumberMasked { get; set; }

    public DateOnly? IdExpiryDate { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Optional photograph, subject to policy (BRD 3). BRD 3 also recommends against storing
    /// the whole ID document, so only the portrait is retained, never a full-document scan.
    /// </summary>
    public byte[]? Photo { get; set; }

    /// <summary>How the identity data was captured, for audit and for BRD 21 feasibility review.</summary>
    public IdCaptureMethod CaptureMethod { get; set; } = IdCaptureMethod.Manual;

    /// <summary>Set by the retention job once the record passes the configured retention period (BRD 22).</summary>
    public DateTimeOffset? PurgedAtUtc { get; set; }

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();

    /// <summary>True when the ID presented has already expired. Feeds the Expired ID Report (BRD 20).</summary>
    public bool IsIdExpired(DateOnly asOf) => IdExpiryDate is { } expiry && expiry < asOf;
}
