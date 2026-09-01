using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

public class Office : AuditableEntity
{
    public Guid FloorId { get; set; }
    public Floor? Floor { get; set; }

    public required string OfficeNo { get; set; }
    public string? Department { get; set; }
}
