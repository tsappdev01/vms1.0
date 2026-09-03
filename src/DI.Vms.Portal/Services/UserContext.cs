using DI.Vms.Domain.Enums;

namespace DI.Vms.Portal.Services;

/// <summary>
/// Per-circuit state for the signed-in operator.
///
/// Scoped, not held on the API client: AddHttpClient registers its typed client as
/// transient, so state stored there would be discarded between calls. In Blazor Server a
/// scoped service lives for the circuit, which is what "the current user" means here.
/// </summary>
public sealed class UserContext
{
    /// <summary>
    /// The role the API is asked to act as. Development authentication reads it from a
    /// request header; when Entra ID is wired up this is replaced by the signed-in
    /// principal and stops being settable from the UI.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.SecuritySupervisor;

    /// <summary>Raised when the role changes, so open pages can reload against it.</summary>
    public event Action? Changed;

    public void SetRole(UserRole role)
    {
        if (Role == role) return;
        Role = role;
        Changed?.Invoke();
    }
}
