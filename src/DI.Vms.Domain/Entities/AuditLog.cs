namespace DI.Vms.Domain.Entities;

/// <summary>
/// Append-only record of every modification (BRD 19). Rows are never updated or deleted
/// outside the retention job.
/// </summary>
/// <remarks>
/// Does not derive from <c>Entity</c>: the audit table is high-volume and append-only, so
/// it uses a monotonic BIGINT identity rather than a GUID, matching db/schema.
/// </remarks>
public class AuditLog
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>e.g. CHECK-IN, CHECK-OUT, UPDATE-HOST, VIEW-UNMASKED-ID.</summary>
    public required string Action { get; set; }

    /// <summary>The entity type affected, e.g. "Visit".</summary>
    public required string EntityName { get; set; }

    /// <summary>
    /// The affected row, where the action targets one. Nullable: some actions (a login,
    /// a failed authentication) have no record, and the column is NULL in the schema.
    /// </summary>
    public Guid? RecordId { get; set; }

    /// <summary>JSON of the changed fields before and after. Must never contain an unmasked ID number.</summary>
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; set; }
    public string? DeviceId { get; set; }
}
