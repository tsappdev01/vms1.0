namespace DI.Vms.Domain.Enums;

/// <summary>The four roles defined in BRD 18, in ascending order of privilege.</summary>
public enum UserRole
{
    SecurityOfficer = 1,
    SecuritySupervisor = 2,
    Admin = 3,
    SystemAdministrator = 4
}
