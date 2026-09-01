using DI.Vms.Domain.Common;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Domain.Entities;

/// <summary>
/// One arrival of one visitor. Carries the chain of custody BRD 26 calls for:
/// Identity -> Company -> DI Entity -> Host -> Purpose -> Check-In -> Location -> Signature -> Check-Out.
/// </summary>
public class Visit : AuditableEntity
{
    /// <summary>Human-readable reference, e.g. <c>VIS-2026-00001245</c> (BRD 7).</summary>
    public required string VisitNumber { get; set; }

    public Guid VisitorId { get; set; }
    public Visitor? Visitor { get; set; }

    public Guid DiEntityId { get; set; }
    public DiEntity? DiEntity { get; set; }

    public Guid HostEmployeeId { get; set; }
    public Employee? HostEmployee { get; set; }

    public string? Purpose { get; set; }
    public VisitorType VisitType { get; set; } = VisitorType.Guest;

    /// <summary>Where the visitor is going. Snapshotted from the host so later host moves do not rewrite history.</summary>
    public string? Floor { get; set; }
    public string? Office { get; set; }
    public string? Department { get; set; }

    // Pre-registration (BRD 16). Null for walk-ins.
    public DateOnly? ExpectedDate { get; set; }
    public TimeOnly? ExpectedTime { get; set; }

    public DateTimeOffset? InTimeUtc { get; set; }
    public DateTimeOffset? OutTimeUtc { get; set; }

    public VisitStatus Status { get; set; } = VisitStatus.Expected;

    public VisitorSignature? Signature { get; set; }

    /// <summary>Tablet that performed the check-in (BRD 7, 19).</summary>
    public string? CheckedInOnDeviceId { get; set; }
    public Guid? CheckedInByUserId { get; set; }
    public Guid? CheckedOutByUserId { get; set; }

    /// <summary>Set by the nightly job when a visit was never checked out (BRD 20).</summary>
    public bool AutoClosed { get; set; }

    /// <summary>Time on the premises, once both stamps exist.</summary>
    public TimeSpan? Duration =>
        InTimeUtc is { } inTime && OutTimeUtc is { } outTime ? outTime - inTime : null;

    public bool IsInside => Status == VisitStatus.Inside;

    /// <summary>
    /// Records the arrival (BRD 7). A signature is mandatory because the visitor's
    /// acknowledgement is part of the chain of custody.
    /// </summary>
    public void CheckIn(Guid userId, string deviceId, VisitorSignature signature, DateTimeOffset nowUtc)
    {
        if (Status == VisitStatus.Inside)
        {
            throw new DomainException($"Visit {VisitNumber} is already checked in.");
        }

        if (Status is VisitStatus.CheckedOut or VisitStatus.Cancelled)
        {
            throw new DomainException($"Visit {VisitNumber} is {Status} and cannot be checked in again.");
        }

        Signature = signature;
        InTimeUtc = nowUtc;
        Status = VisitStatus.Inside;
        CheckedInByUserId = userId;
        CheckedInOnDeviceId = deviceId;
    }

    /// <summary>Records the departure (BRD 10).</summary>
    public void CheckOut(Guid userId, DateTimeOffset nowUtc)
    {
        if (Status != VisitStatus.Inside)
        {
            throw new DomainException($"Visit {VisitNumber} is {Status}; only a visit that is Inside can be checked out.");
        }

        if (nowUtc < InTimeUtc)
        {
            throw new DomainException("Check-out time cannot precede check-in time.");
        }

        OutTimeUtc = nowUtc;
        Status = VisitStatus.CheckedOut;
        CheckedOutByUserId = userId;
    }
}
