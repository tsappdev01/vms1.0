using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DI.Vms.Application.Contracts;
using DI.Vms.Domain.Enums;

namespace DI.Vms.Portal.Services;

/// <summary>
/// Typed client over DI.Vms.Api. Returns the API's own contract types, so a change to
/// a DTO is a compile error here rather than a runtime surprise.
/// </summary>
public sealed class VmsApiClient(HttpClient http, UserContext user, ILogger<VmsApiClient> logger)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<DashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken ct = default) =>
        GetAsync<DashboardSummaryDto>("dashboard/summary", ct);

    public Task<List<VisitListItemDto>?> GetInsideAsync(CancellationToken ct = default) =>
        GetAsync<List<VisitListItemDto>>("visits/inside", ct);

    public Task<List<VisitListItemDto>?> GetExpectedAsync(CancellationToken ct = default) =>
        GetAsync<List<VisitListItemDto>>("visits/expected", ct);

    public Task<List<OccupancyPersonDto>?> GetOccupancyAsync(CancellationToken ct = default) =>
        GetAsync<List<OccupancyPersonDto>>("emergency/occupancy", ct);

    public Task<List<DiEntityDto>?> GetEntitiesAsync(CancellationToken ct = default) =>
        GetAsync<List<DiEntityDto>>("entities", ct);

    public Task<List<EmployeeDto>?> GetEmployeesAsync(string? q = null, CancellationToken ct = default) =>
        GetAsync<List<EmployeeDto>>(string.IsNullOrWhiteSpace(q) ? "employees" : $"employees?q={Uri.EscapeDataString(q)}", ct);

    public Task<List<UserDto>?> GetUsersAsync(CancellationToken ct = default) =>
        GetAsync<List<UserDto>>("users", ct);

    public Task<UserDto?> GetCurrentUserAsync(CancellationToken ct = default) =>
        GetAsync<UserDto>("me", ct);

    public Task<Paged<AuditEntryDto>?> GetAuditAsync(int page = 1, int pageSize = 50, CancellationToken ct = default) =>
        GetAsync<Paged<AuditEntryDto>>($"audit?page={page}&pageSize={pageSize}", ct);

    public Task<Paged<VisitListItemDto>?> SearchVisitsAsync(
        string? q, string? status, string? host, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(q)) query.Add($"q={Uri.EscapeDataString(q)}");
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(host)) query.Add($"host={Uri.EscapeDataString(host)}");
        return GetAsync<Paged<VisitListItemDto>>($"visitors/search?{string.Join('&', query)}", ct);
    }

    public Task<List<ReportDefinition>?> GetReportsAsync(CancellationToken ct = default) =>
        GetAsync<List<ReportDefinition>>("reports", ct);

    public Task<ReportResult?> RunReportAsync(string name, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (from is { } f) query.Add($"from={f:yyyy-MM-dd}");
        if (to is { } t) query.Add($"to={t:yyyy-MM-dd}");
        var qs = query.Count > 0 ? "?" + string.Join('&', query) : string.Empty;
        return GetAsync<ReportResult>($"reports/{name}{qs}", ct);
    }

    public Task<IdentifyResponse?> IdentifyAsync(IdentifyRequest request, CancellationToken ct = default) =>
        PostAsync<IdentifyRequest, IdentifyResponse>("visits/identify", request, ct);

    public Task<CreateVisitResult?> CreateVisitAsync(CreateVisitRequest request, CancellationToken ct = default) =>
        PostAsync<CreateVisitRequest, CreateVisitResult>("visits", request, ct);

    public Task<CheckInResponse?> CheckInAsync(Guid visitId, string signatureImage, string deviceId, CancellationToken ct = default) =>
        PostAsync<CheckInRequest, CheckInResponse>($"visits/{visitId}/check-in", new CheckInRequest(signatureImage, deviceId), ct);

    public Task<CheckOutResponse?> CheckOutAsync(Guid visitId, CancellationToken ct = default) =>
        PostAsync<object, CheckOutResponse>($"visits/{visitId}/check-out", new { }, ct);

    /// <summary>The API returns the new visit's id and status; it has no contract type of its own.</summary>
    public sealed record CreateVisitResult(Guid Id, VisitStatus Status);

    /// <summary>
    /// Retrieves the plaintext ID number. Permission-gated and audited server-side
    /// (BRD 22) - a 403 here is the control working, not a fault.
    /// </summary>
    public async Task<string?> GetUnmaskedIdAsync(Guid visitorId, CancellationToken ct = default)
    {
        using var request = Build(HttpMethod.Get, $"visitors/{visitorId}/id-number");
        using var response = await http.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view unmasked ID numbers.");
        }

        await EnsureOkAsync(response, ct);

        var payload = await response.Content.ReadFromJsonAsync<UnmaskedId>(Json, ct);
        return payload?.IdNumber;
    }

    private sealed record UnmaskedId(string IdNumber);

    private HttpRequestMessage Build(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        // Development authentication only; replaced by a bearer token with Entra ID.
        request.Headers.Add("X-Dev-Role", user.Role.ToString());
        return request;
    }

    private async Task<TOut?> PostAsync<TIn, TOut>(string path, TIn body, CancellationToken ct)
    {
        try
        {
            using var request = Build(HttpMethod.Post, path);
            request.Content = JsonContent.Create(body, options: Json);
            using var response = await http.SendAsync(request, ct);
            await EnsureOkAsync(response, ct);
            return await response.Content.ReadFromJsonAsync<TOut>(Json, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "POST {Path} failed", path);
            throw new VmsApiUnavailableException(
                $"The API did not respond. Check that DI.Vms.Api is running at {http.BaseAddress}.", ex);
        }
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            using var request = Build(HttpMethod.Get, path);
            using var response = await http.SendAsync(request, ct);
            await EnsureOkAsync(response, ct);
            return await response.Content.ReadFromJsonAsync<T>(Json, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            /* A page that renders an empty table when the API is down is worse than one
               that says so, so this is rethrown with something a user can act on. */
            logger.LogWarning(ex, "GET {Path} failed", path);
            throw new VmsApiUnavailableException(
                $"The API did not respond. Check that DI.Vms.Api is running at {http.BaseAddress}.", ex);
        }
    }

    private static async Task EnsureOkAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        // The API returns RFC 7807; its detail is more useful than the status line.
        var detail = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemShape>(Json, ct);
            if (!string.IsNullOrWhiteSpace(problem?.Detail)) detail = problem.Detail;
            else if (!string.IsNullOrWhiteSpace(problem?.Title)) detail = problem.Title;
        }
        catch
        {
            /* Not a problem+json body; the status line stands. */
        }

        throw new VmsApiException(detail);
    }

    private sealed record ProblemShape(string? Title, string? Detail);
}

public class VmsApiException(string message) : Exception(message);

public sealed class VmsApiUnavailableException(string message, Exception inner)
    : VmsApiException(message)
{
    public Exception Inner { get; } = inner;
}
