namespace DI.Vms.Application.Abstractions;

/// <summary>Allocates the human-readable visit reference, e.g. VIS-2026-00001245 (BRD 7).</summary>
public interface IVisitNumberGenerator
{
    Task<string> NextAsync(CancellationToken ct = default);
}
