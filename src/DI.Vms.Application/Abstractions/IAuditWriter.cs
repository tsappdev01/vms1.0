namespace DI.Vms.Application.Abstractions;

/// <summary>Writes the append-only audit trail (BRD 19).</summary>
public interface IAuditWriter
{
    /// <summary>
    /// Records an action. Values must already be free of unmasked ID numbers - the audit
    /// trail is widely readable and must not become a second copy of the sensitive data.
    /// </summary>
    Task WriteAsync(
        string action,
        string entityName,
        Guid? recordId,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken ct = default);
}
