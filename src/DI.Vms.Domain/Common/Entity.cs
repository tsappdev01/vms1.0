namespace DI.Vms.Domain.Common;

/// <summary>Base type for all persisted entities.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

/// <summary>Adds the creation/modification stamps that BRD 19 requires for auditing.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public Guid? ModifiedByUserId { get; set; }
}
