namespace DI.Vms.Blazor.Services;

/// <summary>
/// The purposes the desk offers.
///
/// Held in code rather than in a table: unlike the entity list, which follows the group's
/// companies and must change without a redeploy, this is a closed vocabulary belonging to
/// the form. Moving it to vms.VisitPurpose is small if it needs to be editable at the desk.
///
/// It lives here rather than in the page because the tablet asks for it over the API. Two
/// copies would drift, and the drift would show up as a visit recorded with a purpose the
/// report does not group by.
/// </summary>
public static class VisitPurposes
{
    public const string Other = "Other";

    public static readonly string[] All =
    [
        "Meetings", "Submission", "Delivery", "Collection",
        "Visit", "Interview", "Enquiry", "Payments",
    ];
}
