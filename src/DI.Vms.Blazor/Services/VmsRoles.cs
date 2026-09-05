namespace DI.Vms.Blazor.Services;

/// <summary>
/// The app roles, and the policies built from them.
///
/// The role names are Entra ID **app roles** rather than security groups. A group
/// membership arrives as an object id that means nothing when read in a log or a policy,
/// and the token stops carrying them once a user is in enough groups. App roles arrive as
/// their own value, in the <c>roles</c> claim, and say what they are.
///
/// The division follows BRD §18 and docs/06-security-privacy-rbac.md rather than being
/// invented here: an officer checks visitors in, a supervisor also reads the report.
/// </summary>
public static class VmsRoles
{
    public const string Officer = "Vms.Officer";
    public const string Supervisor = "Vms.Supervisor";
    public const string Admin = "Vms.Admin";
    public const string SystemAdmin = "Vms.SystemAdmin";

    /// <summary>
    /// Seeing an unmasked Emirates ID number. Deliberately not implied by any of the
    /// above: §22 is explicit that seniority is not the same thing as a need to see the
    /// number, so this is granted per person and audited on use.
    ///
    /// Nothing enforces it yet - the numbers are still stored and shown in full. It is
    /// declared here so the grant exists in Entra from the start, and so the masking work
    /// has a role to hang on rather than needing a directory change on the day.
    /// </summary>
    public const string UnmaskedId = "Vms.UnmaskedId";

    /// <summary>Check a visitor in. Everyone who works a desk.</summary>
    public const string CanCheckIn = nameof(CanCheckIn);

    /// <summary>Read the visitor report. Supervisor and above.</summary>
    public const string CanViewReport = nameof(CanViewReport);

    /// <summary>See an Emirates ID number in full. Nothing uses this yet.</summary>
    public const string CanViewUnmaskedId = nameof(CanViewUnmaskedId);

    public static readonly string[] All =
        [Officer, Supervisor, Admin, SystemAdmin, UnmaskedId];
}
