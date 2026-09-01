using DI.Vms.Domain.Common;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Domain.Entities;

/// <summary>
/// An operator of the system (BRD 17, 18). Authentication is delegated to Microsoft
/// Entra ID (BRD 23); this record carries the local role and reception assignment.
/// </summary>
public class User : AuditableEntity
{
    public required string Username { get; set; }
    public required string Name { get; set; }
    public UserRole Role { get; set; } = UserRole.SecurityOfficer;

    /// <summary>Entra ID object identifier, when the account is federated.</summary>
    public string? ExternalObjectId { get; set; }

    /// <summary>The reception point this user normally operates, e.g. "DIP Main Reception".</summary>
    public string? SecurityLocation { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this user may see unmasked ID numbers. Off by default: BRD 22 requires that
    /// normal Security users see only the masked form.
    /// </summary>
    public bool CanViewUnmaskedId { get; set; }
}
