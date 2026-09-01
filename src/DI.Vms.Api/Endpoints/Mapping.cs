using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Entities;

namespace DI.Vms.Api.Endpoints;

/// <summary>Entity to DTO projections. Nothing here exposes an unmasked ID number.</summary>
internal static class Mapping
{
    public static VisitListItemDto ToDto(this Visit v, DateOnly today) => new(
        v.Id,
        v.VisitNumber,
        v.Visitor?.Name ?? string.Empty,
        v.Visitor?.Company,
        v.Visitor?.IdType ?? default,
        v.Visitor?.IdNumberMasked ?? string.Empty,
        v.Visitor?.IsIdExpired(today) ?? false,
        v.HostEmployee?.Name ?? string.Empty,
        v.DiEntity?.EntityName ?? string.Empty,
        v.Department,
        v.Floor,
        v.Office,
        v.Purpose,
        v.VisitType,
        v.InTimeUtc,
        v.OutTimeUtc,
        v.ExpectedDate,
        v.ExpectedTime,
        v.Status);

    public static DiEntityDto ToDto(this DiEntity e) =>
        new(e.Id, e.EntityCode, e.EntityName, e.IsActive);

    public static EmployeeDto ToDto(this Employee e) => new(
        e.Id, e.EmployeeCode, e.Name,
        e.DiEntity?.EntityName ?? string.Empty,
        e.Department, e.Designation,
        e.Floor?.FloorNo, e.Office?.OfficeNo,
        e.Email, e.Mobile, e.IsActive);

    public static UserDto ToDto(this User u) => new(
        u.Id, u.Username, u.Name, u.Role, u.SecurityLocation, u.CanViewUnmaskedId, u.IsActive);
}
