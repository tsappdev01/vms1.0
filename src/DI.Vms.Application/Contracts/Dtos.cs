using DI.Vms.Domain.Enums;

namespace DI.Vms.Application.Contracts;

/* Response shapes for docs/03-api-specification.md.
   No DTO carries an unmasked ID number: the only route to plaintext is the
   dedicated, permission-gated, audited endpoint. */

public sealed record CardVerificationDto(
    bool IsGenuine,
    string CardStatus,
    DateTimeOffset? VerifiedAtUtc,
    bool VgAvailable);

public sealed record IdentifyRequest(
    IdType IdType,
    string IdNumber,
    IdCaptureMethod CaptureMethod,
    CardVerificationDto? CardVerification);

public sealed record IdentifiedVisitorDto(
    Guid Id,
    string Name,
    string? Company,
    string IdNumberMasked,
    DateOnly? IdExpiryDate,
    bool IdExpired,
    int TotalVisits,
    DateOnly? LastVisitDate);

public sealed record IdentifyResponse(bool Found, IdentifiedVisitorDto? Visitor);

public sealed record AddressDto(
    string? Emirate,
    string? City,
    string? Area,
    string? Street,
    string? Building,
    string? Flat,
    string? PoBox,
    string? Mobile,
    string? Email);

public sealed record NewVisitorDto(
    string Name,
    string? Company,
    IdType IdType,
    string IdNumber,
    DateOnly? IdExpiryDate,
    string? Nationality,
    DateOnly? DateOfBirth,
    string? Photo,
    IdCaptureMethod CaptureMethod,
    AddressDto? Address = null);

public sealed record CreateVisitRequest(
    NewVisitorDto? Visitor,
    Guid? VisitorId,
    Guid DiEntityId,
    Guid HostEmployeeId,
    string? Purpose,
    VisitorType VisitType,
    DateOnly? ExpectedDate,
    TimeOnly? ExpectedTime);

public sealed record CheckInRequest(string SignatureImage, string DeviceId);

public sealed record HostSummaryDto(string Name, bool Notified);

public sealed record CheckInResponse(
    string VisitNumber,
    DateTimeOffset InTimeUtc,
    string Status,
    HostSummaryDto Host);

public sealed record CheckOutResponse(
    DateTimeOffset OutTimeUtc,
    int DurationMinutes,
    string Status);

public sealed record VisitListItemDto(
    Guid Id,
    string VisitNumber,
    string VisitorName,
    string? Company,
    IdType IdType,
    string IdNumberMasked,
    bool IdExpired,
    string HostName,
    string EntityName,
    string? Department,
    string? Floor,
    string? Office,
    string? Purpose,
    VisitorType VisitType,
    DateTimeOffset? InTimeUtc,
    DateTimeOffset? OutTimeUtc,
    DateOnly? ExpectedDate,
    TimeOnly? ExpectedTime,
    VisitStatus Status,
    string? Nationality = null,
    string? Address = null,
    string? PhotoBase64 = null);

public sealed record EntityCountDto(string EntityName, int Visitors);

public sealed record DashboardSummaryDto(
    int TotalToday,
    int CurrentlyInside,
    int CheckedOut,
    int Expected,
    IReadOnlyList<EntityCountDto> VisitorsByEntity);

public sealed record OccupancyPersonDto(
    string Name,
    string? Company,
    string? HostName,
    string? Floor,
    string Category,
    DateTimeOffset? InTimeUtc,
    string? VisitNumber);

public sealed record VisitorProfileDto(
    Guid Id,
    string Name,
    string? Company,
    IdType IdType,
    string IdNumberMasked,
    DateOnly? IdExpiryDate,
    bool IdExpired,
    string? Nationality,
    int TotalVisits,
    DateOnly? LastVisitDate,
    IReadOnlyList<VisitListItemDto> Visits);

public sealed record DiEntityDto(Guid Id, string EntityCode, string EntityName, bool IsActive);

public sealed record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string Name,
    string EntityName,
    string? Department,
    string? Designation,
    string? Floor,
    string? Office,
    string? Email,
    string? Mobile,
    bool IsActive);

public sealed record UserDto(
    Guid Id,
    string Username,
    string Name,
    UserRole Role,
    string? SecurityLocation,
    bool CanViewUnmaskedId,
    bool IsActive);

public sealed record AuditEntryDto(
    long Id,
    string UserName,
    string Action,
    string EntityName,
    string? RecordRef,
    string? OldValue,
    string? NewValue,
    DateTimeOffset TimestampUtc,
    string? IpAddress,
    string? DeviceId);

public sealed record Paged<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

/* ------------------------------------------------------------------- reports */

/// <summary>
/// A report as a generic table. Reports differ only in their query, so one shape lets the
/// portal render and export any of them without a bespoke screen each (BRD 20).
/// </summary>
public sealed record ReportResult(
    string Name,
    string Title,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows,
    DateOnly? From,
    DateOnly? To,
    DateTimeOffset GeneratedAtUtc);

public sealed record ReportDefinition(string Name, string Title, string Description, bool TakesDateRange);
