using DI.Vms.Application.Abstractions;
using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Enums;
using DI.Vms.Domain.ValueObjects;
using DI.Vms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Api.Endpoints;

public static class VisitorEndpoints
{
    private const int MaxPageSize = 200;

    public static void MapVisitorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/visitors/search", async (
            VmsDbContext db,
            IIdProtector protector,
            string? q,
            string? idNumber,
            string? company,
            string? host,
            VisitStatus? status,
            string? floor,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize,
            CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > MaxPageSize ? 25 : pageSize;

            var query = db.Visits
                .AsNoTracking()
                .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(v =>
                    v.Visitor!.Name.Contains(term) ||
                    (v.Visitor.Company != null && v.Visitor.Company.Contains(term)) ||
                    v.VisitNumber.Contains(term));
            }

            /* Searching by ID number hashes the input and matches the index. The
               plaintext is never compared against the database and never logged. */
            if (!string.IsNullOrWhiteSpace(idNumber))
            {
                var normalised = IdNumber.Normalise(idNumber, IdType.EmiratesId);
                var hash = protector.Hash(normalised);
                query = query.Where(v => v.Visitor!.IdNumberHash == hash);
            }

            if (!string.IsNullOrWhiteSpace(company))
            {
                query = query.Where(v => v.Visitor!.Company != null && v.Visitor.Company.Contains(company));
            }

            if (!string.IsNullOrWhiteSpace(host))
            {
                query = query.Where(v => v.HostEmployee!.Name.Contains(host));
            }

            if (status is { } s) query = query.Where(v => v.Status == s);
            if (!string.IsNullOrWhiteSpace(floor)) query = query.Where(v => v.Floor == floor);

            if (from is { } f)
            {
                var fromUtc = new DateTimeOffset(f.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                query = query.Where(v => v.InTimeUtc >= fromUtc);
            }

            if (to is { } t)
            {
                var toUtc = new DateTimeOffset(t.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
                query = query.Where(v => v.InTimeUtc <= toUtc);
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(v => v.InTimeUtc ?? v.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return Results.Ok(new Paged<VisitListItemDto>(
                items.Select(v => v.ToDto(today)).ToList(), page, pageSize, total));
        })
        .WithName("SearchVisitors")
        .WithTags("Visitors");

        app.MapGet("/api/v1/visitors/{id:guid}/history", async (
            Guid id, VmsDbContext db, CancellationToken ct) =>
        {
            var visitor = await db.Visitors.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
            if (visitor is null) return Results.NotFound();

            var visits = await db.Visits
                .AsNoTracking()
                .Include(v => v.Visitor).Include(v => v.HostEmployee).Include(v => v.DiEntity)
                .Where(v => v.VisitorId == id)
                .OrderByDescending(v => v.InTimeUtc)
                .ToListAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var last = visits.FirstOrDefault(v => v.InTimeUtc != null)?.InTimeUtc;

            return Results.Ok(new VisitorProfileDto(
                visitor.Id, visitor.Name, visitor.Company, visitor.IdType,
                visitor.IdNumberMasked, visitor.IdExpiryDate, visitor.IsIdExpired(today),
                visitor.Nationality, visits.Count,
                last is { } l ? DateOnly.FromDateTime(l.UtcDateTime) : null,
                visits.Select(v => v.ToDto(today)).ToList()));
        })
        .WithName("GetVisitorHistory")
        .WithTags("Visitors");

        /* The only path to a plaintext ID number (BRD 22). Permission-gated, and the read
           itself is audited - looking at an ID number is an auditable event. */
        app.MapGet("/api/v1/visitors/{id:guid}/id-number", async (
            Guid id,
            VmsDbContext db,
            IIdProtector protector,
            ICurrentUser user,
            IAuditWriter audit,
            CancellationToken ct) =>
        {
            if (!user.CanViewUnmaskedId)
            {
                return Results.Problem(
                    title: "Not permitted",
                    detail: "You do not have permission to view unmasked ID numbers.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            var visitor = await db.Visitors.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (visitor is null) return Results.NotFound();

            await audit.WriteAsync("VIEW-UNMASKED-ID", "Visitor", visitor.Id, null, visitor.Name, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { idNumber = protector.Decrypt(visitor.IdNumberCipher) });
        })
        .WithName("GetUnmaskedIdNumber")
        .WithTags("Visitors");
    }
}
