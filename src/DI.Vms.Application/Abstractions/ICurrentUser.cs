using DI.Vms.Domain.Enums;

namespace DI.Vms.Application.Abstractions;

/// <summary>The authenticated operator behind the current request.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    string Name { get; }
    UserRole Role { get; }

    /// <summary>BRD 22: a separate per-user grant, never implied by a role.</summary>
    bool CanViewUnmaskedId { get; }

    string? IpAddress { get; }
    string? DeviceId { get; }
}
