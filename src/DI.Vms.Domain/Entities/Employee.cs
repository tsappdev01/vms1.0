using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

/// <summary>
/// The host master (BRD 5). Security searches this rather than typing host details,
/// and department/floor/office auto-populate from the match.
/// </summary>
public class Employee : AuditableEntity
{
    public required string EmployeeCode { get; set; }
    public required string Name { get; set; }

    public Guid DiEntityId { get; set; }
    public DiEntity? DiEntity { get; set; }

    public string? Department { get; set; }
    public string? Designation { get; set; }

    public Guid? FloorId { get; set; }
    public Floor? Floor { get; set; }

    public Guid? OfficeId { get; set; }
    public Office? Office { get; set; }

    public string? Email { get; set; }
    public string? Mobile { get; set; }

    public bool IsActive { get; set; } = true;
}
