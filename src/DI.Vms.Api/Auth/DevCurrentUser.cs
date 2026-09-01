using DI.Vms.Application.Abstractions;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Api.Auth;

/// <summary>
/// Development stand-in for the signed-in operator.
///
/// Real authentication is Microsoft Entra ID (BRD 23), which needs an app registration
/// that does not exist yet. Until then this reads the role from an X-Dev-Role header so
/// the authorisation matrix can be exercised end to end. It is registered ONLY when
/// Auth:Mode is "Development", and the API refuses to start in Production with it enabled.
/// </summary>
public sealed class DevCurrentUser : ICurrentUser
{
    public DevCurrentUser(IHttpContextAccessor accessor)
    {
        var http = accessor.HttpContext;
        var header = http?.Request.Headers["X-Dev-Role"].ToString();

        Role = Enum.TryParse<UserRole>(header, ignoreCase: true, out var parsed)
            ? parsed
            : UserRole.SecuritySupervisor;

        // Stable per-role identifier so audit rows are attributable across requests.
        UserId = new Guid($"00000000-0000-0000-0000-0000000000{(int)Role:D2}");
        Username = $"dev-{Role.ToString().ToLowerInvariant()}";
        Name = $"Development {Role}";

        // Mirrors the production rule: an explicit grant, not implied by seniority.
        CanViewUnmaskedId = Role is UserRole.SecuritySupervisor or UserRole.SystemAdministrator;

        IpAddress = http?.Connection.RemoteIpAddress?.ToString();
        DeviceId = http?.Request.Headers["X-Device-Id"].ToString() is { Length: > 0 } d ? d : "PORTAL";
    }

    public Guid UserId { get; }
    public string Username { get; }
    public string Name { get; }
    public UserRole Role { get; }
    public bool CanViewUnmaskedId { get; }
    public string? IpAddress { get; }
    public string? DeviceId { get; }
}
