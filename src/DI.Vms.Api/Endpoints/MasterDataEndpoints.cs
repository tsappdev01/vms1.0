using DI.Vms.Application.Abstractions;
using DI.Vms.Application.Contracts;
using DI.Vms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Api.Endpoints;

public static class MasterDataEndpoints
{
    public static void MapMasterDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/me", (ICurrentUser user) => Results.Ok(new UserDto(
            user.UserId, user.Username, user.Name, user.Role, null, user.CanViewUnmaskedId, true)))
            .WithName("GetCurrentUser")
            .WithTags("Master data");

        app.MapGet("/api/v1/entities", async (VmsDbContext db, CancellationToken ct) =>
            Results.Ok(await db.DiEntities.AsNoTracking()
                .OrderBy(e => e.EntityName)
                .Select(e => new DiEntityDto(e.Id, e.EntityCode, e.EntityName, e.IsActive))
                .ToListAsync(ct)))
            .WithName("GetEntities")
            .WithTags("Master data");

        /* Host search (BRD 5). Reception hits this on every registration, so it is
           deliberately narrow: active hosts only, capped result set. */
        app.MapGet("/api/v1/employees", async (VmsDbContext db, string? q, CancellationToken ct) =>
        {
            var query = db.Employees
                .AsNoTracking()
                .Include(e => e.DiEntity).Include(e => e.Floor).Include(e => e.Office)
                .Where(e => e.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(e => e.Name.Contains(term) || e.EmployeeCode.Contains(term));
            }

            var employees = await query.OrderBy(e => e.Name).Take(50).ToListAsync(ct);
            return Results.Ok(employees.Select(e => e.ToDto()).ToList());
        })
        .WithName("GetEmployees")
        .WithTags("Master data");

        app.MapGet("/api/v1/users", async (VmsDbContext db, CancellationToken ct) =>
        {
            var users = await db.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);
            return Results.Ok(users.Select(u => u.ToDto()).ToList());
        })
        .WithName("GetUsers")
        .WithTags("Master data");

        app.MapGet("/api/v1/audit", async (
            VmsDbContext db, string? entity, Guid? recordId, DateOnly? from, DateOnly? to,
            int page, int pageSize, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            var query = db.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(a => a.EntityName == entity);
            if (recordId is { } r) query = query.Where(a => a.RecordId == r);
            if (from is { } f) query = query.Where(a => a.TimestampUtc >= new DateTimeOffset(f.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
            if (to is { } t) query = query.Where(a => a.TimestampUtc <= new DateTimeOffset(t.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero));

            var total = await query.CountAsync(ct);

            /* Left join, not inner: an audit row whose user has since been removed - or
               which was written by an identity not in the User table - must still appear.
               An audit trail that silently drops rows is worse than none, because it
               looks complete. */
            var rows = await query
                .OrderByDescending(a => a.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .GroupJoin(db.Users.AsNoTracking(), a => a.UserId, u => u.Id,
                    (a, users) => new { Audit = a, Users = users })
                .SelectMany(x => x.Users.DefaultIfEmpty(),
                    (x, u) => new { x.Audit, UserName = u != null ? u.Username : null })
                .ToListAsync(ct);

            var items = rows.Select(x => new AuditEntryDto(
                x.Audit.Id, x.UserName ?? x.Audit.UserId.ToString(), x.Audit.Action, x.Audit.EntityName,
                x.Audit.RecordId?.ToString(), x.Audit.OldValue, x.Audit.NewValue,
                x.Audit.TimestampUtc, x.Audit.IpAddress, x.Audit.DeviceId)).ToList();

            return Results.Ok(new Paged<AuditEntryDto>(items, page, pageSize, total));
        })
        .WithName("GetAudit")
        .WithTags("Master data");
    }
}
