using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

/// <summary>A company within the Dubai Investments group (BRD 6).</summary>
public class DiEntity : AuditableEntity
{
    public required string EntityCode { get; set; }
    public required string EntityName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
