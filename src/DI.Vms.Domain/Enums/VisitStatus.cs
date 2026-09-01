namespace DI.Vms.Domain.Enums;

/// <summary>Lifecycle of a single visit (BRD 7, 10, 16).</summary>
public enum VisitStatus
{
    /// <summary>Pre-registered by a host, visitor has not arrived (BRD 16).</summary>
    Expected = 1,

    /// <summary>Checked in and on the premises (BRD 7).</summary>
    Inside = 2,

    /// <summary>Checked out (BRD 10).</summary>
    CheckedOut = 3,

    /// <summary>Expected visit that never happened; closed off by the nightly job.</summary>
    Cancelled = 4
}
