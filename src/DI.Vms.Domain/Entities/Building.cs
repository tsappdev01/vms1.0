using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

/// <summary>A physical building. Multi-building support is Phase 3 (BRD 25).</summary>
public class Building : AuditableEntity
{
    public required string BuildingName { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
