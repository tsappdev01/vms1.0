using System.Globalization;
using System.Text;
using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Enums;
using DI.Vms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Api.Endpoints;

/* The BRD 20 report set. Every report is a query returning a table, so one endpoint
   and one generic shape serve all of them - the portal renders and exports any report
   without a bespoke screen each.

   ID numbers appear only in their masked form. An unmasked spreadsheet leaving the
   building would defeat the control in BRD 22 entirely, so exports obey the same
   masking as the screen. */

public static class ReportEndpoints
{
    private static readonly ReportDefinition[] Definitions =
    [
        new("visitor-details-by-entity", "Visitor Details by Entity",
            "Every recorded field for each visit, grouped by DI entity, with check-in and check-out timestamps.", true),
        new("daily-visitors", "Daily Visitor Report", "Visitor, company, ID type, ID number, entity, host, in, out, duration.", true),
        new("by-entity", "Visitor by Entity", "Visit counts grouped by DI entity.", true),
        new("by-host", "Visitor by Host", "Visit counts grouped by host employee.", true),
        new("by-company", "Visitor by Company", "Visit counts grouped by visiting company.", true),
        new("by-date", "Visitor by Date", "Visit counts per day.", true),
        new("currently-inside", "Currently Inside", "Point-in-time occupancy snapshot.", false),
        new("never-checked-out", "Visitors Never Checked Out", "Visits closed by the nightly job, or still open from a previous day.", true),
        new("frequent-visitors", "Frequent Visitors", "Visitors ranked by visit count.", true),
        new("expired-id", "Expired ID Report", "Visitors whose presented ID has expired.", false),
        new("by-floor", "Visitor Activity by Floor", "Visit counts grouped by floor.", true),
        new("user-activity", "Security User Activity", "Actions per security user, from the audit trail.", true),
    ];

    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reports", () => Results.Ok(Definitions))
            .WithName("GetReportDefinitions")
            .WithTags("Reports");

        app.MapGet("/api/v1/reports/{name}", async (
            string name,
            VmsDbContext db,
            DateOnly? from,
            DateOnly? to,
            string? format,
            CancellationToken ct) =>
        {
            var definition = Definitions.FirstOrDefault(d =>
                string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

            if (definition is null)
            {
                return Results.Problem(
                    title: "Unknown report",
                    detail: $"No report named '{name}'. Call GET /api/v1/reports for the list.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            /* Default to today so an unbounded query cannot be issued by accident;
               a full-history scan of the visit table is not something a report screen
               should be able to trigger with an empty form. */
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var fromDate = from ?? today;
            var toDate = to ?? today;

            if (toDate < fromDate)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["to"] = ["'to' cannot be earlier than 'from'."],
                });
            }

            var result = await BuildAsync(definition, db, fromDate, toDate, ct);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csv = ToCsv(result);
                return Results.File(
                    Encoding.UTF8.GetBytes(csv),
                    "text/csv",
                    $"{definition.Name}-{fromDate:yyyy-MM-dd}.csv");
            }

            return Results.Ok(result);
        })
        .WithName("RunReport")
        .WithTags("Reports");
    }

    private static async Task<ReportResult> BuildAsync(
        ReportDefinition definition,
        VmsDbContext db,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var inRange = db.Visits.AsNoTracking()
            .Where(v => v.InTimeUtc >= fromUtc && v.InTimeUtc <= toUtc);

        string[] columns;
        List<IReadOnlyList<string?>> rows;

        switch (definition.Name)
        {
            case "visitor-details-by-entity":
            {
                /* The full record per visit, ordered by entity. ID numbers stay masked:
                   an unmasked spreadsheet leaving the building would defeat BRD 22, and
                   an export is exactly how that happens. */
                columns =
                [
                    "DI Entity", "Visit No.", "Visitor", "Company", "Nationality",
                    "ID Type", "ID Number", "ID Expiry", "Date of Birth", "Home Address", "Mobile", "Email",
                    "Visitor Type", "Host", "Department", "Floor", "Office", "Purpose",
                    "Identity Source", "Check-In", "Check-Out", "Duration", "Status", "Registered",
                ];

                var data = await inRange
                    .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                    .OrderBy(v => v.DiEntity!.EntityName).ThenBy(v => v.InTimeUtc)
                    .ToListAsync(ct);

                rows = data.Select(v => (IReadOnlyList<string?>)new[]
                {
                    v.DiEntity?.EntityName,
                    v.VisitNumber,
                    v.Visitor?.Name,
                    v.Visitor?.Company,
                    v.Visitor?.Nationality,
                    v.Visitor?.IdType.ToString(),
                    v.Visitor?.IdNumberMasked,
                    v.Visitor?.IdExpiryDate?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
                    v.Visitor?.DateOfBirth?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
                    v.Visitor?.Address?.ToString(),
                    v.Visitor?.Address?.Mobile,
                    v.Visitor?.Address?.Email,
                    v.VisitType.ToString(),
                    v.HostEmployee?.Name,
                    v.Department,
                    v.Floor,
                    v.Office,
                    v.Purpose,
                    v.Visitor?.CaptureMethod == IdCaptureMethod.Manual ? "Manual entry" : "Card chip",
                    Gst(v.InTimeUtc, "dd-MMM-yyyy HH:mm"),
                    Gst(v.OutTimeUtc, "dd-MMM-yyyy HH:mm"),
                    Duration(v.Duration),
                    v.Status.ToString(),
                    Gst(v.CreatedAtUtc, "dd-MMM-yyyy HH:mm"),
                }).ToList();
                break;
            }

            case "daily-visitors":
            {
                columns = ["Visit No.", "Date", "Visitor", "Company", "ID Type", "ID Number", "DI Entity", "Host", "In", "Out", "Duration"];
                var data = await inRange
                    .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                    .OrderBy(v => v.InTimeUtc)
                    .ToListAsync(ct);
                rows = data.Select(v => (IReadOnlyList<string?>)new[]
                {
                    v.VisitNumber,
                    Gst(v.InTimeUtc, "dd-MMM-yyyy"),
                    v.Visitor?.Name,
                    v.Visitor?.Company,
                    v.Visitor?.IdType.ToString(),
                    v.Visitor?.IdNumberMasked,
                    v.DiEntity?.EntityName,
                    v.HostEmployee?.Name,
                    Gst(v.InTimeUtc, "HH:mm"),
                    Gst(v.OutTimeUtc, "HH:mm"),
                    Duration(v.Duration),
                }).ToList();
                break;
            }

            case "by-entity":
            {
                columns = ["DI Entity", "Visitors"];
                var data = await inRange
                    .GroupBy(v => v.DiEntity!.EntityName)
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync(ct);
                rows = data.Select(x => (IReadOnlyList<string?>)new[] { x.Key, x.Count.ToString(CultureInfo.InvariantCulture) }).ToList();
                break;
            }

            case "by-host":
            {
                columns = ["Host", "Department", "Visitors"];
                var data = await inRange
                    .GroupBy(v => new { v.HostEmployee!.Name, v.HostEmployee.Department })
                    .Select(g => new { g.Key.Name, g.Key.Department, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync(ct);
                rows = data.Select(x => (IReadOnlyList<string?>)new[] { x.Name, x.Department, x.Count.ToString(CultureInfo.InvariantCulture) }).ToList();
                break;
            }

            case "by-company":
            {
                columns = ["Company", "Visitors"];
                var data = await inRange
                    .Where(v => v.Visitor!.Company != null)
                    .GroupBy(v => v.Visitor!.Company!)
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync(ct);
                rows = data.Select(x => (IReadOnlyList<string?>)new[] { x.Key, x.Count.ToString(CultureInfo.InvariantCulture) }).ToList();
                break;
            }

            case "by-date":
            {
                columns = ["Date", "Visitors"];
                var data = await inRange
                    .GroupBy(v => v.InTimeUtc!.Value.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Day)
                    .ToListAsync(ct);
                rows = data.Select(x => (IReadOnlyList<string?>)new[]
                {
                    x.Day.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
                    x.Count.ToString(CultureInfo.InvariantCulture),
                }).ToList();
                break;
            }

            case "currently-inside":
            {
                columns = ["Visit No.", "Visitor", "Company", "Host", "DI Entity", "Floor", "In", "Duration"];
                var data = await db.Visits.AsNoTracking()
                    .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                    .Where(v => v.Status == VisitStatus.Inside)
                    .OrderBy(v => v.Floor).ThenBy(v => v.InTimeUtc)
                    .ToListAsync(ct);
                rows = data.Select(v => (IReadOnlyList<string?>)new[]
                {
                    v.VisitNumber, v.Visitor?.Name, v.Visitor?.Company, v.HostEmployee?.Name,
                    v.DiEntity?.EntityName, v.Floor, Gst(v.InTimeUtc, "HH:mm"),
                    Duration(DateTimeOffset.UtcNow - v.InTimeUtc),
                }).ToList();
                break;
            }

            case "never-checked-out":
            {
                /* Visits that were auto-closed, plus anything still Inside from before
                   today - both are the same operational problem: a visitor whose exit
                   was never recorded. */
                columns = ["Visit No.", "Date", "Visitor", "Host", "In", "Auto-closed"];
                var startOfToday = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                var data = await db.Visits.AsNoTracking()
                    .Include(v => v.Visitor).Include(v => v.HostEmployee)
                    .Where(v => v.InTimeUtc >= fromUtc && v.InTimeUtc <= toUtc)
                    .Where(v => v.AutoClosed || (v.Status == VisitStatus.Inside && v.InTimeUtc < startOfToday))
                    .OrderBy(v => v.InTimeUtc)
                    .ToListAsync(ct);
                rows = data.Select(v => (IReadOnlyList<string?>)new[]
                {
                    v.VisitNumber, Gst(v.InTimeUtc, "dd-MMM-yyyy"), v.Visitor?.Name,
                    v.HostEmployee?.Name, Gst(v.InTimeUtc, "HH:mm"), v.AutoClosed ? "Yes" : "No",
                }).ToList();
                break;
            }

            case "frequent-visitors":
            {
                columns = ["Visitor", "Company", "ID Number", "Visits", "Last Visit"];
                var data = await inRange
                    .GroupBy(v => new { v.VisitorId, v.Visitor!.Name, v.Visitor.Company, v.Visitor.IdNumberMasked })
                    .Select(g => new
                    {
                        g.Key.Name, g.Key.Company, g.Key.IdNumberMasked,
                        Count = g.Count(),
                        Last = g.Max(x => x.InTimeUtc),
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(200)
                    .ToListAsync(ct);
                rows = data.Select(x => (IReadOnlyList<string?>)new[]
                {
                    x.Name, x.Company, x.IdNumberMasked,
                    x.Count.ToString(CultureInfo.InvariantCulture),
                    Gst(x.Last, "dd-MMM-yyyy"),
                }).ToList();
                break;
            }

            case "expired-id":
            {
                columns = ["Visitor", "Company", "ID Type", "ID Number", "Expired On", "Total Visits"];
                var data = await db.Visitors.AsNoTracking()
                    .Where(v => v.IdExpiryDate != null && v.IdExpiryDate < today)
                    .Select(v => new
                    {
                        v.Name, v.Company, v.IdType, v.IdNumberMasked, v.IdExpiryDate,
                        Visits = db.Visits.Count(x => x.VisitorId == v.Id),
                    })
                    .OrderBy(v => v.IdExpiryDate)
                    .ToListAsync(ct);
                rows = data.Select(v => (IReadOnlyList<string?>)new[]
                {
                    v.Name, v.Company, v.IdType.ToString(), v.IdNumberMasked,
                    v.IdExpiryDate?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
                    v.Visits.ToString(CultureInfo.InvariantCulture),
                }).ToList();
                break;
            }

            case "by-floor":
            {
                columns = ["Floor", "Visitors"];
                var data = await inRange
                    .GroupBy(v => v.Floor)
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .ToListAsync(ct);
                rows = data
                    .OrderBy(x => int.TryParse(x.Key, out var n) ? n : int.MaxValue)
                    .Select(x => (IReadOnlyList<string?>)new[] { x.Key ?? "(not recorded)", x.Count.ToString(CultureInfo.InvariantCulture) })
                    .ToList();
                break;
            }

            case "user-activity":
            {
                columns = ["User", "Action", "Count"];
                var data = await db.AuditLogs.AsNoTracking()
                    .Where(a => a.TimestampUtc >= fromUtc && a.TimestampUtc <= toUtc)
                    .GroupBy(a => new { a.UserId, a.Action })
                    .Select(g => new { g.Key.UserId, g.Key.Action, Count = g.Count() })
                    .ToListAsync(ct);

                var names = await db.Users.AsNoTracking()
                    .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

                rows = data
                    .OrderByDescending(x => x.Count)
                    .Select(x => (IReadOnlyList<string?>)new[]
                    {
                        names.TryGetValue(x.UserId, out var n) ? n : x.UserId.ToString(),
                        x.Action,
                        x.Count.ToString(CultureInfo.InvariantCulture),
                    })
                    .ToList();
                break;
            }

            default:
                columns = [];
                rows = [];
                break;
        }

        return new ReportResult(
            definition.Name, definition.Title, columns, rows,
            definition.TakesDateRange ? from : null,
            definition.TakesDateRange ? to : null,
            DateTimeOffset.UtcNow);
    }

    /// <summary>Renders a UTC instant in Gulf Standard Time, which is what a reader expects.</summary>
    private static string? Gst(DateTimeOffset? instant, string format) =>
        instant?.ToOffset(TimeSpan.FromHours(4)).ToString(format, CultureInfo.InvariantCulture);

    private static string? Duration(TimeSpan? span) =>
        span is null ? null : $"{(int)span.Value.TotalHours}h {span.Value.Minutes}m";

    private static string ToCsv(ReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{report.Title}");
        if (report.From is { } f && report.To is { } t)
        {
            sb.AppendLine($"{f:dd-MMM-yyyy} to {t:dd-MMM-yyyy}");
        }
        sb.AppendLine($"Generated {report.GeneratedAtUtc.ToOffset(TimeSpan.FromHours(4)):dd-MMM-yyyy HH:mm} GST");
        sb.AppendLine();
        sb.AppendLine(string.Join(',', report.Columns.Select(Escape)));
        foreach (var row in report.Rows)
        {
            sb.AppendLine(string.Join(',', row.Select(Escape)));
        }
        return sb.ToString();
    }

    /// <summary>
    /// RFC 4180 quoting. Also neutralises values a spreadsheet would treat as a formula:
    /// a company name beginning with '=' must not execute when the export is opened.
    /// </summary>
    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var v = value;
        if (v[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            v = "'" + v;
        }

        return v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r')
            ? '"' + v.Replace("\"", "\"\"") + '"'
            : v;
    }
}
