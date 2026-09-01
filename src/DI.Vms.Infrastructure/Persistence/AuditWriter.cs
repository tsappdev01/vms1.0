using DI.Vms.Application.Abstractions;
using DI.Vms.Domain.Entities;

namespace DI.Vms.Infrastructure.Persistence;

/// <summary>
/// Appends to the audit trail (BRD 19). Rows are added to the change tracker; the calling
/// use case saves them in the same transaction as the change being audited, so an action
/// and its audit record cannot diverge.
/// </summary>
public sealed class AuditWriter(VmsDbContext db, ICurrentUser user) : IAuditWriter
{
    public Task WriteAsync(
        string action,
        string entityName,
        Guid? recordId,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = user.UserId,
            Action = action,
            EntityName = entityName,
            RecordId = recordId,
            OldValue = oldValue,
            NewValue = newValue,
            TimestampUtc = DateTimeOffset.UtcNow,
            IpAddress = user.IpAddress,
            DeviceId = user.DeviceId,
        });

        _ = ct;
        return Task.CompletedTask;
    }
}
