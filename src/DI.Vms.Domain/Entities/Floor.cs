using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

public class Floor : AuditableEntity
{
    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }

    public required string FloorNo { get; set; }
    public string? FloorName { get; set; }

    public ICollection<Office> Offices { get; set; } = new List<Office>();
}
