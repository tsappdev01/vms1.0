using System.Security.Claims;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Blazor.Api;

/// <summary>
/// The API the Android reception app talks to.
///
/// It exists because a tablet must not reach SQL Server. A connection string on a device
/// that leaves the building is a credential that has left the building, and the database
/// would then be exposed to anything that could reach it rather than to one server.
///
/// The trust model is not new. A card read arriving from a tablet is the same problem as
/// one arriving from the desk browser through ICP's agent: an untrusted client claiming to
/// have read a card. So it uses the same <see cref="AgentCardReader"/> - the server issues
/// the request ID, checks it back, verifies the signature over the response, and parses
/// every field out of the signed XML rather than out of anything the client sent
/// separately. One implementation, one set of rules, and no second way in.
/// </summary>
public static class VisitsApi
{
    /// <summary>
    /// Bearer tokens, not the cookie the browser uses. The two live side by side: this
    /// group requires a token from Entra with the API's own scope, so a session cookie
    /// stolen from a desk browser cannot be replayed against the API.
    /// </summary>
    private static readonly AuthorizeAttribute TabletPolicy = new()
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = VmsRoles.CanCheckIn,
    };

    public static void MapVisitsApi(this IEndpointRouteBuilder routes)
    {
        var api = routes.MapGroup("/api").RequireAuthorization(TabletPolicy);

        /* Everything reception needs to fill the form, in one call. A tablet on office
           wifi is not a desk on ethernet, and three round trips to draw one screen is
           three chances to be halfway through. */
        api.MapGet("/reference", async (IDbContextFactory<VmsDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);

            var entities = await db.DiEntities
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new EntityDto(e.Id, e.Name))
                .ToListAsync(ct);

            return Results.Ok(new ReferenceDto(entities, VisitPurposes.All, VisitPurposes.Other));
        });

        /* The host list, searched server-side. Not shipped to the device: it is 725 people
           with names, titles, email addresses and employers, and that is a staff
           directory sitting on a tablet at a reception desk. */
        api.MapGet("/people", async (
            string? q,
            int? entityId,
            bool allEntities,
            IDbContextFactory<VmsDbContext> factory,
            CancellationToken ct) =>
        {
            var term = (q ?? string.Empty).Trim();
            if (term.Length < 2) return Results.Ok(Array.Empty<PersonDto>());

            await using var db = await factory.CreateDbContextAsync(ct);

            var pattern = LikeLiteral(term);

            var all = db.People.Where(p => p.IsActive
                && (EF.Functions.Like(p.DisplayName, $"%{pattern}%", LikeEscape)
                 || (p.Email != null && EF.Functions.Like(p.Email, $"{pattern}%", LikeEscape))));

            if (!allEntities && entityId is { } id and > 0)
            {
                all = all.Where(p => p.DiEntityId == id);
            }

            var people = await all
                .OrderBy(p => p.DisplayName)
                .Take(12)
                .Select(p => new PersonDto(p.Id, p.DisplayName, p.Title, p.Email, p.CompanyName))
                .ToListAsync(ct);

            return Results.Ok(people);
        });

        /* Start a read. The device gets an ID it cannot have chosen, spends it once, and
           cannot prepare a response before being asked for one. */
        api.MapPost("/reads", (AgentCardReader reader) =>
            Results.Ok(new ReadTicketDto(reader.BeginRead())));

        /* Finish it. The body carries the signed XML and the visit details - never the
           card fields, which are read out of the XML here. */
        api.MapPost("/visits", async (
            SaveVisitRequest request,
            AgentCardReader reader,
            IDbContextFactory<VmsDbContext> factory,
            ClaimsPrincipal user,
            ILoggerFactory loggers,
            CancellationToken ct) =>
        {
            var log = loggers.CreateLogger(typeof(VisitsApi));

            if (request.EntityId <= 0) return Problem("An entity is required.");
            if (string.IsNullOrWhiteSpace(request.PersonToVisit)) return Problem("A person to visit is required.");
            if (string.IsNullOrWhiteSpace(request.Purpose)) return Problem("A purpose is required.");

            if (request.Purpose == VisitPurposes.Other && string.IsNullOrWhiteSpace(request.PurposeOther))
            {
                return Problem("Details are required when the purpose is Other.");
            }

            CardData card;
            string captureMethod;

            if (request.Manual is { } manual)
            {
                /* Typed in, because the chip would not read. Marked as such, and never
                   allowed to arrive alongside a card read - one entry, one provenance. */
                if (request.ReadResponseXml is not null)
                {
                    return Problem("Send either a card read or manual details, not both.");
                }

                var digits = new string(manual.IdNumber?.Where(char.IsAsciiDigit).ToArray() ?? []);
                if (digits.Length == 0) return Problem("An ID number is required.");
                if (string.IsNullOrWhiteSpace(manual.FullNameEnglish)) return Problem("A name is required.");

                card = new CardData
                {
                    IdNumber = digits,
                    CardNumber = manual.CardNumber,
                    FullNameEnglish = CardResponseParser.CleanName(manual.FullNameEnglish),
                    FullNameArabic = CardResponseParser.CleanName(manual.FullNameArabic),
                    NationalityEnglish = manual.NationalityEnglish,
                    DateOfBirth = manual.DateOfBirth,
                    ExpiryDate = manual.ExpiryDate,
                    AddressMobile = manual.AddressMobile,
                };
                captureMethod = "Manual";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.RequestId) || request.ReadResponseXml is null)
                {
                    return Problem("A card read needs both the request ID it was issued for and the response.");
                }

                try
                {
                    card = reader.Complete(request.RequestId, request.ReadResponseXml);
                }
                catch (InvalidOperationException ex)
                {
                    /* The message is written for a person at a desk, so it is safe to
                       pass on - it says what is wrong with the read, not how the check
                       works. */
                    log.LogWarning("Rejected a tablet card read: {Reason}", ex.Message);
                    return Problem(ex.Message);
                }

                captureMethod = card.SignatureWarning is null ? "CardReader" : "CardReaderUnverified";
            }

            await using var db = await factory.CreateDbContextAsync(ct);

            var host = request.PersonToVisitId is { } personId
                ? await db.People.FirstOrDefaultAsync(p => p.Id == personId, ct)
                : null;

            var entry = new VisitorEntry
            {
                IdNumber = FieldLengths.Clamp(card.IdNumber, FieldLengths.IdNumber) ?? string.Empty,
                CardNumber = FieldLengths.Clamp(card.CardNumber, FieldLengths.CardNumber),
                Photo = card.Photo,
                IdType = FieldLengths.Clamp(card.IdType, FieldLengths.CardField),
                IssueDate = FieldLengths.Clamp(card.IssueDate, FieldLengths.CardField),
                ExpiryDate = FieldLengths.Clamp(card.ExpiryDate, FieldLengths.CardField),
                FullNameEnglish = FieldLengths.Clamp(card.FullNameEnglish, FieldLengths.Name) ?? string.Empty,
                FullNameRaw = FieldLengths.Clamp(card.FullNameRaw, FieldLengths.Name),
                FullNameArabic = FieldLengths.Clamp(card.FullNameArabic, FieldLengths.Name),
                TitleEnglish = FieldLengths.Clamp(card.TitleEnglish, FieldLengths.CardField),
                Gender = FieldLengths.Clamp(card.Gender, FieldLengths.CardField),
                DateOfBirth = FieldLengths.Clamp(card.DateOfBirth, FieldLengths.CardField),
                NationalityEnglish = FieldLengths.Clamp(card.NationalityEnglish, FieldLengths.CardField),
                NationalityCode = FieldLengths.Clamp(card.NationalityCode, FieldLengths.CardField),
                PlaceOfBirthEnglish = FieldLengths.Clamp(card.PlaceOfBirthEnglish, FieldLengths.CardField),
                AddressEmirate = FieldLengths.Clamp(card.AddressEmirate, FieldLengths.CardField),
                AddressCity = FieldLengths.Clamp(card.AddressCity, FieldLengths.CardField),
                AddressArea = FieldLengths.Clamp(card.AddressArea, FieldLengths.CardField),
                AddressStreet = FieldLengths.Clamp(card.AddressStreet, FieldLengths.CardField),
                AddressBuilding = FieldLengths.Clamp(card.AddressBuilding, FieldLengths.CardField),
                AddressPoBox = FieldLengths.Clamp(card.AddressPoBox, FieldLengths.CardField),
                AddressPhone = FieldLengths.Clamp(card.AddressPhone, FieldLengths.CardField),
                AddressMobile = FieldLengths.Clamp(card.AddressMobile, FieldLengths.CardField),
                AddressEmail = FieldLengths.Clamp(card.AddressEmail, FieldLengths.Email),

                DiEntityId = request.EntityId,
                PersonToVisit = FieldLengths.Clamp(request.PersonToVisit!.Trim(), FieldLengths.PersonToVisit) ?? string.Empty,
                PersonToVisitId = host?.Id,
                PersonToVisitTitle = FieldLengths.Clamp(host?.Title, FieldLengths.Title),
                PersonToVisitEmail = FieldLengths.Clamp(host?.Email, FieldLengths.Email),
                PersonToVisitCompany = FieldLengths.Clamp(host?.CompanyName, FieldLengths.Company),
                Purpose = FieldLengths.Clamp(request.Purpose!, FieldLengths.Purpose) ?? string.Empty,
                PurposeOther = request.Purpose == VisitPurposes.Other
                    ? FieldLengths.Clamp(request.PurposeOther?.Trim(), FieldLengths.PurposeOther)
                    : null,
                RecordedAtUtc = DateTimeOffset.UtcNow,
                CaptureMethod = captureMethod,
                RecordedBy = FieldLengths.Clamp(SignedInAs(user), FieldLengths.RecordedBy),
            };

            db.VisitorEntries.Add(entry);
            await db.SaveChangesAsync(ct);

            log.LogInformation(
                "Visit {Id} recorded from the tablet by {User}, capture {Capture}.",
                entry.Id, entry.RecordedBy ?? "(unknown)", entry.CaptureMethod);

            return Results.Ok(new SavedVisitDto(entry.Id, entry.RecordedAtUtc, entry.CaptureMethod, card.SignatureWarning));
        });
    }

    /// <summary>
    /// The user principal name, not the display name: two people share a display name
    /// often enough that an audit trail cannot afford it.
    /// </summary>
    private static string? SignedInAs(ClaimsPrincipal user) =>
        user.FindFirst("preferred_username")?.Value
        ?? user.FindFirst(ClaimTypes.Upn)?.Value
        ?? user.Identity?.Name;

    /// <summary>
    /// A refusal the device can show as it stands. 400 rather than 500: these are all
    /// things about the request, and a tablet retrying a 500 forever would be reasonable
    /// behaviour on its part.
    /// </summary>
    private static IResult Problem(string detail) =>
        Results.Problem(detail, statusCode: StatusCodes.Status400BadRequest, title: "The visit was not saved");

    private const string LikeEscape = "\\";

    /// <summary>
    /// Escapes what LIKE treats as pattern syntax. Not injection - EF parameterises the
    /// term - but a typed % would otherwise match the entire staff list.
    /// </summary>
    private static string LikeLiteral(string term) => term
        .Replace(LikeEscape, LikeEscape + LikeEscape)
        .Replace("%", LikeEscape + "%")
        .Replace("_", LikeEscape + "_")
        .Replace("[", LikeEscape + "[");
}

public sealed record EntityDto(int Id, string Name);

public sealed record PersonDto(int Id, string DisplayName, string? Title, string? Email, string? CompanyName);

public sealed record ReferenceDto(IReadOnlyList<EntityDto> Entities, IReadOnlyList<string> Purposes, string OtherPurpose);

public sealed record ReadTicketDto(string RequestId);

public sealed record SavedVisitDto(int Id, DateTimeOffset RecordedAtUtc, string CaptureMethod, string? Warning);

/// <summary>What reception typed when the chip would not read.</summary>
public sealed record ManualIdentity(
    string? IdNumber,
    string? CardNumber,
    string? FullNameEnglish,
    string? FullNameArabic,
    string? NationalityEnglish,
    string? DateOfBirth,
    string? ExpiryDate,
    string? AddressMobile);

/// <summary>
/// One visit. Either <see cref="ReadResponseXml"/> with the request ID it was issued for,
/// or <see cref="Manual"/> - never both, because an entry has one provenance.
///
/// Deliberately no card fields of its own. Everything about the visitor comes out of the
/// signed XML on the server; a name in this body would be a name the server took on trust
/// from a tablet, and the signature would be decoration.
/// </summary>
public sealed record SaveVisitRequest(
    string? RequestId,
    string? ReadResponseXml,
    ManualIdentity? Manual,
    int EntityId,
    string? PersonToVisit,
    int? PersonToVisitId,
    string? Purpose,
    string? PurposeOther);
