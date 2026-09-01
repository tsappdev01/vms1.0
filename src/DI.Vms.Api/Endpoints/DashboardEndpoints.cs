using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Enums;
using DI.Vms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/dashboard/summary", async (VmsDbContext db, CancellationToken ct) =>
        {
            var since = DateTimeOffset.UtcNow.AddHours(4).Date.AddHours(-4); // start of today, GST
            var from = new DateTimeOffset(since, TimeSpan.Zero);

            var todays = db.Visits.Where(v => v.InTimeUtc >= from);

            var inside = await todays.CountAsync(v => v.Status == VisitStatus.Inside, ct);
            var checkedOut = await todays.CountAsync(v => v.Status == VisitStatus.CheckedOut, ct);
            var expected = await db.Visits.CountAsync(v => v.Status == VisitStatus.Expected, ct);

            var byEntity = await todays
                .GroupBy(v => v.DiEntity!.EntityName)
                .Select(g => new EntityCountDto(g.Key, g.Count()))
                .OrderByDescending(x => x.Visitors)
                .ToListAsync(ct);

            return Results.Ok(new DashboardSummaryDto(inside + checkedOut, inside, checkedOut, expected, byEntity));
        })
        .WithName("GetDashboardSummary")
        .WithTags("Dashboard");

        app.MapGet("/api/v1/visits/inside", async (VmsDbContext db, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var visits = await db.Visits
                .AsNoTracking()
                .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                .Where(v => v.Status == VisitStatus.Inside)
                .OrderBy(v => v.InTimeUtc)
                .ToListAsync(ct);

            return Results.Ok(visits.Select(v => v.ToDto(today)).ToList());
        })
        .WithName("GetVisitsInside")
        .WithTags("Dashboard");

        app.MapGet("/api/v1/visits/expected", async (VmsDbContext db, DateOnly? date, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var query = db.Visits
                .AsNoTracking()
                .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                .Where(v => v.Status == VisitStatus.Expected);

            if (date is { } d)
            {
                query = query.Where(v => v.ExpectedDate == d);
            }

            var visits = await query.OrderBy(v => v.ExpectedDate).ThenBy(v => v.ExpectedTime).ToListAsync(ct);
            return Results.Ok(visits.Select(v => v.ToDto(today)).ToList());
        })
        .WithName("GetExpectedVisits")
        .WithTags("Dashboard");

        /* Unpaged and open to every authenticated role: during an evacuation nobody
           should be blocked by pagination or a permission check (BRD 12). */
        app.MapGet("/api/v1/emergency/occupancy", async (VmsDbContext db, CancellationToken ct) =>
        {
            var people = await db.Visits
                .AsNoTracking()
                .Include(v => v.Visitor).Include(v => v.HostEmployee)
                .Where(v => v.Status == VisitStatus.Inside)
                .OrderBy(v => v.Floor).ThenBy(v => v.InTimeUtc)
                .Select(v => new OccupancyPersonDto(
                    v.Visitor!.Name,
                    v.Visitor.Company,
                    v.HostEmployee!.Name,
                    v.Floor,
                    v.VisitType == VisitorType.Contractor ? "Contractor"
                        : v.VisitType == VisitorType.ServiceProvider ? "ServiceProvider"
                        : "Visitor",
                    v.InTimeUtc,
                    v.VisitNumber))
                .ToListAsync(ct);

            return Results.Ok(people);
        })
        .WithName("GetOccupancy")
        .WithTags("Emergency");
    }
}
