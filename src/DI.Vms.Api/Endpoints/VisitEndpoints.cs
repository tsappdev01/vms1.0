using DI.Vms.Application.Abstractions;
using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Common;
using DI.Vms.Domain.Entities;
using DI.Vms.Domain.Enums;
using DI.Vms.Domain.ValueObjects;
using DI.Vms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Api.Endpoints;

public static class VisitEndpoints
{
    public static void MapVisitEndpoints(this IEndpointRouteBuilder app)
    {
        /* Repeat-visitor recognition (BRD 14). The client sends fields already extracted
           from the chip; the server never sees the card. */
        app.MapPost("/api/v1/visits/identify", async (
            IdentifyRequest request,
            VmsDbContext db,
            IIdProtector protector,
            CancellationToken ct) =>
        {
            IdNumber id;
            try
            {
                id = IdNumber.Create(request.IdNumber, request.IdType);
            }
            catch (DomainException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["idNumber"] = [ex.Message],
                });
            }

            var hash = protector.Hash(id.Value);
            var visitor = await db.Visitors
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdType == request.IdType && v.IdNumberHash == hash, ct);

            if (visitor is null)
            {
                return Results.Ok(new IdentifyResponse(false, null));
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var total = await db.Visits.CountAsync(v => v.VisitorId == visitor.Id, ct);
            var last = await db.Visits
                .Where(v => v.VisitorId == visitor.Id && v.InTimeUtc != null)
                .OrderByDescending(v => v.InTimeUtc)
                .Select(v => v.InTimeUtc)
                .FirstOrDefaultAsync(ct);

            return Results.Ok(new IdentifyResponse(true, new IdentifiedVisitorDto(
                visitor.Id,
                visitor.Name,
                visitor.Company,
                visitor.IdNumberMasked,
                visitor.IdExpiryDate,
                visitor.IsIdExpired(today),
                total,
                last is { } l ? DateOnly.FromDateTime(l.UtcDateTime) : null)));
        })
        .WithName("IdentifyVisitor")
        .WithTags("Visits");

        /* Creates the visit in Expected. Floor, office and department are snapshotted from
           the host rather than accepted from the client, so they cannot be spoofed. */
        app.MapPost("/api/v1/visits", async (
            CreateVisitRequest request,
            VmsDbContext db,
            IIdProtector protector,
            ICurrentUser user,
            IAuditWriter audit,
            IVisitNumberGenerator numbers,
            CancellationToken ct) =>
        {
            var host = await db.Employees
                .Include(e => e.Floor).Include(e => e.Office)
                .FirstOrDefaultAsync(e => e.Id == request.HostEmployeeId, ct);

            if (host is null)
            {
                return Results.Problem(
                    title: "Unknown host", detail: "No employee matches hostEmployeeId.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!await db.DiEntities.AnyAsync(e => e.Id == request.DiEntityId, ct))
            {
                return Results.Problem(
                    title: "Unknown DI entity", detail: "No entity matches diEntityId.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            Visitor visitor;

            if (request.VisitorId is { } existingId)
            {
                var found = await db.Visitors.FirstOrDefaultAsync(v => v.Id == existingId, ct);
                if (found is null)
                {
                    return Results.Problem(
                        title: "Unknown visitor", detail: "No visitor matches visitorId.",
                        statusCode: StatusCodes.Status400BadRequest);
                }
                visitor = found;
            }
            else if (request.Visitor is { } incoming)
            {
                IdNumber id;
                try
                {
                    id = IdNumber.Create(incoming.IdNumber, incoming.IdType);
                }
                catch (DomainException ex)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["visitor.idNumber"] = [ex.Message],
                    });
                }

                var hash = protector.Hash(id.Value);
                var existing = await db.Visitors
                    .FirstOrDefaultAsync(v => v.IdType == incoming.IdType && v.IdNumberHash == hash, ct);

                if (existing is not null)
                {
                    visitor = existing;
                }
                else
                {
                    visitor = new Visitor
                    {
                        Name = incoming.Name,
                        Company = incoming.Company,
                        IdType = incoming.IdType,
                        IdNumberCipher = protector.Encrypt(id.Value),
                        IdNumberHash = hash,
                        IdNumberMasked = id.Masked,
                        IdExpiryDate = incoming.IdExpiryDate,
                        Nationality = incoming.Nationality,
                        DateOfBirth = incoming.DateOfBirth,
                        Photo = string.IsNullOrWhiteSpace(incoming.Photo)
                            ? null
                            : Convert.FromBase64String(incoming.Photo),
                        CaptureMethod = incoming.CaptureMethod,
                        CreatedByUserId = user.UserId,
                    };
                    db.Visitors.Add(visitor);
                }
            }
            else
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["visitor"] = ["Supply either visitorId or a visitor object."],
                });
            }

            var visit = new Visit
            {
                /* Allocated here rather than at check-in: VisitNumber is UNIQUE, so a
                   placeholder would collide the moment a second visit was pre-registered. */
                VisitNumber = await numbers.NextAsync(ct),
                Visitor = visitor,
                DiEntityId = request.DiEntityId,
                HostEmployeeId = host.Id,
                Purpose = request.Purpose,
                VisitType = request.VisitType,
                Department = host.Department,
                Floor = host.Floor?.FloorNo,
                Office = host.Office?.OfficeNo,
                ExpectedDate = request.ExpectedDate,
                ExpectedTime = request.ExpectedTime,
                Status = VisitStatus.Expected,
                CreatedByUserId = user.UserId,
            };

            db.Visits.Add(visit);
            await audit.WriteAsync("CREATE-VISIT", nameof(Visit), visit.Id, null, host.Name, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/visits/{visit.Id}", new { visit.Id, visit.Status });
        })
        .WithName("CreateVisit")
        .WithTags("Visits");

        app.MapPost("/api/v1/visits/{id:guid}/check-in", async (
            Guid id,
            CheckInRequest request,
            VmsDbContext db,
            ICurrentUser user,
            IAuditWriter audit,
            CancellationToken ct) =>
        {
            var visit = await db.Visits
                .Include(v => v.HostEmployee)
                .Include(v => v.Signature)
                .FirstOrDefaultAsync(v => v.Id == id, ct);

            if (visit is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.SignatureImage))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["signatureImage"] = ["A signature is required at check-in."],
                });
            }

            var signature = new VisitorSignature
            {
                VisitId = visit.Id,
                Image = Convert.FromBase64String(request.SignatureImage),
                DeviceId = request.DeviceId,
            };

            try
            {
                visit.CheckIn(user.UserId, request.DeviceId, signature, DateTimeOffset.UtcNow);
            }
            catch (DomainException ex)
            {
                return Results.Problem(
                    title: "Check-in not allowed", detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }

            await audit.WriteAsync("CHECK-IN", nameof(Visit), visit.Id, null, visit.VisitNumber, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new CheckInResponse(
                visit.VisitNumber,
                visit.InTimeUtc!.Value,
                visit.Status.ToString(),
                new HostSummaryDto(visit.HostEmployee?.Name ?? string.Empty, false)));
        })
        .WithName("CheckIn")
        .WithTags("Visits");

        app.MapPost("/api/v1/visits/{id:guid}/check-out", async (
            Guid id,
            VmsDbContext db,
            ICurrentUser user,
            IAuditWriter audit,
            CancellationToken ct) =>
        {
            var visit = await db.Visits.FirstOrDefaultAsync(v => v.Id == id, ct);
            if (visit is null) return Results.NotFound();

            try
            {
                visit.CheckOut(user.UserId, DateTimeOffset.UtcNow);
            }
            catch (DomainException ex)
            {
                return Results.Problem(
                    title: "Check-out not allowed", detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }

            await audit.WriteAsync("CHECK-OUT", nameof(Visit), visit.Id, "Inside", "CheckedOut", ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new CheckOutResponse(
                visit.OutTimeUtc!.Value,
                (int)(visit.Duration?.TotalMinutes ?? 0),
                visit.Status.ToString()));
        })
        .WithName("CheckOut")
        .WithTags("Visits");
    }
}
