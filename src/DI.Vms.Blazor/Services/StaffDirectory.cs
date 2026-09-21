using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DI.Vms.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// One person as Entra ID holds them. Not <see cref="Person"/>: this has no database
/// identity, and the difference matters at the point a visit is saved.
/// </summary>
public sealed record DirectoryPerson(
    string ObjectId,
    string DisplayName,
    string? Title,
    string? Email,
    string? CompanyName);

/// <summary>The host list, wherever it comes from.</summary>
public interface IStaffDirectory
{
    /// <summary>False when the host list is the database table and this is not in play.</summary>
    bool Enabled { get; }

    /// <summary>
    /// The people matching <paramref name="term"/>, or the head of the whole directory when
    /// it is empty - which is what makes the picker list the tenant on focus rather than
    /// demanding a guess first.
    /// </summary>
    Task<IReadOnlyList<DirectoryPerson>> SearchAsync(string term, CancellationToken ct);

    /// <summary>
    /// One person by object ID, or null if the directory no longer holds them. Used where a
    /// pick is redeemed, so what is stored comes from the directory rather than from
    /// whatever the client said about them.
    /// </summary>
    Task<DirectoryPerson?> FindAsync(string objectId, CancellationToken ct);

    /// <summary>How many people the directory currently holds, for the diagnostics screen.</summary>
    int Count { get; }
}

/// <summary>Always empty. Registered when the host list is the database table.</summary>
public sealed class NoStaffDirectory : IStaffDirectory
{
    public bool Enabled => false;
    public int Count => 0;

    public Task<IReadOnlyList<DirectoryPerson>> SearchAsync(string term, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DirectoryPerson>>([]);

    public Task<DirectoryPerson?> FindAsync(string objectId, CancellationToken ct) =>
        Task.FromResult<DirectoryPerson?>(null);
}

/// <summary>
/// The Entra ID tenant, read through Microsoft Graph with the application's own identity.
///
/// It fetches the directory once and searches it in memory, rather than turning every
/// keystroke into a Graph query. A type-ahead at a reception desk issues a request every
/// few characters; multiplied by visitors and by desks that is a great many calls against
/// a tenant-wide throttle, for a list that changes when somebody joins. So: one paged
/// fetch, held for <see cref="DirectoryOptions.CacheMinutes"/>, matched locally.
///
/// It also means the picker can show the directory before anything is typed, and that a
/// Graph outage costs the desk nothing until the copy expires - and even then the last
/// copy is kept and served rather than the picker going blank. A host list slightly out of
/// date is a far smaller problem at a desk than no host list.
/// </summary>
public sealed class EntraStaffDirectory : IStaffDirectory, IDisposable
{
    public const string HttpClientName = "graph";

    private readonly DirectoryOptions options;
    private readonly IHttpClientFactory clients;
    private readonly ILogger<EntraStaffDirectory> log;

    /// <summary>
    /// MSAL, not a hand-rolled token request: it caches the token, refreshes it before it
    /// expires and backs off when the token endpoint throttles. One instance, because the
    /// cache lives inside it.
    /// </summary>
    private readonly IConfidentialClientApplication? app;

    /// <summary>One fetch at a time. Ten browser circuits waking to an expired copy must not become ten fetches.</summary>
    private readonly SemaphoreSlim refreshing = new(1, 1);

    private IReadOnlyList<DirectoryPerson> people = [];
    private DateTimeOffset fetchedAtUtc = DateTimeOffset.MinValue;

    public EntraStaffDirectory(
        DirectoryOptions options,
        IHttpClientFactory clients,
        ILogger<EntraStaffDirectory> log)
    {
        this.options = options;
        this.clients = clients;
        this.log = log;

        if (!options.UsesEntraId) return;

        if (!options.CredentialsPresent)
        {
            /* Not thrown: the desk can still take visitors with a typed host name, and an
               app that refuses to start because a directory setting is missing is a worse
               outcome than one that says so on every screen that needs it. */
            log.LogError(
                "Directory:Source is EntraId but the tenant, client ID or client secret is not set. " +
                "The host list will be empty. Set Directory:TenantId, Directory:ClientId and " +
                "Directory:ClientSecret - or the AzureAd equivalents - in the Web App's configuration.");
            return;
        }

        app = ConfidentialClientApplicationBuilder
            .Create(options.ClientId)
            .WithClientSecret(options.ClientSecret)
            .WithAuthority($"{options.Instance.TrimEnd('/')}/{options.TenantId}")
            .Build();
    }

    public bool Enabled => app is not null;

    public int Count => people.Count;

    /// <summary>When the copy was taken, for the diagnostics screen. Null if it never was.</summary>
    public DateTimeOffset? FetchedAtUtc => fetchedAtUtc == DateTimeOffset.MinValue ? null : fetchedAtUtc;

    /// <summary>The last failure, so a screen can say why the list is empty rather than just being empty.</summary>
    public string? LastError { get; private set; }

    public async Task<IReadOnlyList<DirectoryPerson>> SearchAsync(string term, CancellationToken ct)
    {
        if (!Enabled) return [];

        var all = await CurrentAsync(ct);
        term = term.Trim();

        if (term.Length == 0)
        {
            return all.Take(options.MaximumSuggestions).ToList();
        }

        /* Matched anywhere in the name and at the start of the address, which is what the
           database search did - a visitor asking for "Kamau" should find Emma Kamau.
           Ordinal-ignore-case rather than the current culture: the directory holds Arabic
           and Latin names side by side, and a culture-sensitive comparison over a list this
           size on every keystroke is measurably slower for no benefit here. */
        var matches = all
            .Where(p => p.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (p.Email is not null && p.Email.StartsWith(term, StringComparison.OrdinalIgnoreCase)))
            .Take(options.MaximumSuggestions)
            .ToList();

        return matches;
    }

    public async Task<DirectoryPerson?> FindAsync(string objectId, CancellationToken ct)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(objectId)) return null;

        var all = await CurrentAsync(ct);
        return all.FirstOrDefault(p => p.ObjectId == objectId);
    }

    /// <summary>
    /// The held copy, refetched if it has expired. A failed refetch keeps the copy it had:
    /// stale names beat no names.
    /// </summary>
    private async Task<IReadOnlyList<DirectoryPerson>> CurrentAsync(CancellationToken ct)
    {
        var age = DateTimeOffset.UtcNow - fetchedAtUtc;
        if (age < TimeSpan.FromMinutes(options.CacheMinutes)) return people;

        await refreshing.WaitAsync(ct);
        try
        {
            // Another caller may have refreshed it while this one waited.
            if (DateTimeOffset.UtcNow - fetchedAtUtc < TimeSpan.FromMinutes(options.CacheMinutes)) return people;

            var fetched = await FetchAsync(ct);

            people = fetched;
            fetchedAtUtc = DateTimeOffset.UtcNow;
            LastError = null;

            log.LogInformation("Read {Count} people from the Entra ID directory.", fetched.Count);
            return people;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;

            /* Held back rather than retried on the next keystroke: a directory that is
               refusing us is refusing us, and hammering it during an outage is how a
               throttle becomes a lockout. Half the cache window, so it recovers on its own
               without the desk having to restart anything. */
            fetchedAtUtc = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(options.CacheMinutes / 2.0);

            log.LogError(ex,
                "Could not read the Entra ID directory. {Count} name(s) from the last successful read are still being used.",
                people.Count);

            return people;
        }
        finally
        {
            refreshing.Release();
        }
    }

    /// <summary>
    /// Every enabled member of the tenant, following Graph's paging.
    ///
    /// accountEnabled filters leavers whose account is disabled but not yet deleted.
    /// Guests and service accounts are not filtered here: a guest can be somebody's host,
    /// and a rule that guesses which accounts are people belongs in the tenant, not in a
    /// visitor book.
    /// </summary>
    private async Task<IReadOnlyList<DirectoryPerson>> FetchAsync(CancellationToken ct)
    {
        var token = await TokenAsync(ct);

        using var http = clients.CreateClient(HttpClientName);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url =
            $"{options.GraphBaseUrl.TrimEnd('/')}/v1.0/users" +
            "?$select=id,displayName,jobTitle,mail,userPrincipalName,companyName" +
            "&$filter=accountEnabled eq true" +
            "&$orderby=displayName" +
            "&$count=true" +
            "&$top=999";

        // $orderby and $filter together over users is an advanced query, and Graph rejects
        // it without this header rather than ignoring it.
        http.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

        var collected = new List<DirectoryPerson>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? next = url;
        var pages = 0;

        while (next is not null && collected.Count < options.MaximumUsers)
        {
            using var response = await http.GetAsync(next, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);

                /* The body is Graph's own error, which names the permission when the
                   permission is what is wrong - "Authorization_RequestDenied" is the one
                   that means User.Read.All was never consented to. Worth logging in full
                   and worth trimming, because a Graph error body can be long. */
                throw new InvalidOperationException(
                    $"Microsoft Graph answered {(int)response.StatusCode} {response.ReasonPhrase}. " +
                    body[..Math.Min(body.Length, 600)]);
            }

            var page = await response.Content.ReadFromJsonAsync<GraphUserPage>(cancellationToken: ct)
                       ?? throw new InvalidOperationException("Microsoft Graph returned an empty page of users.");

            foreach (var user in page.Value ?? [])
            {
                if (string.IsNullOrWhiteSpace(user.Id)) continue;

                // A directory entry with no display name is not something a receptionist
                // can pick, so the address is the next best label and neither is a loss.
                var name = user.DisplayName
                           ?? user.Mail
                           ?? user.UserPrincipalName;

                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!seen.Add(user.Id)) continue;

                collected.Add(new DirectoryPerson(
                    ObjectId: user.Id,
                    DisplayName: FieldLengths.Clamp(name, FieldLengths.PersonToVisit)!,
                    Title: FieldLengths.Clamp(user.JobTitle, FieldLengths.Title),
                    Email: FieldLengths.Clamp(user.Mail ?? user.UserPrincipalName, FieldLengths.Email),
                    CompanyName: FieldLengths.Clamp(user.CompanyName, FieldLengths.Company)));
            }

            next = page.NextLink;
            pages++;

            // Graph pages at 999; a tenant needing more than this many is one where the
            // ceiling above is the thing to raise, deliberately.
            if (pages > 200) break;
        }

        if (collected.Count >= options.MaximumUsers)
        {
            log.LogWarning(
                "Stopped reading the directory at {Max} people (Directory:MaximumUsers). The host list is incomplete.",
                options.MaximumUsers);
        }

        return collected;
    }

    private async Task<string> TokenAsync(CancellationToken ct)
    {
        if (app is null) throw new InvalidOperationException("The directory is not configured.");

        /* .default with an application permission: the token carries whatever the
           registration has been consented to, which should be User.Read.All and nothing
           else. Asking for a narrower scope here would not narrow it. */
        var scope = $"{options.GraphBaseUrl.TrimEnd('/')}/.default";

        var result = await app.AcquireTokenForClient([scope]).ExecuteAsync(ct);
        return result.AccessToken;
    }

    public void Dispose() => refreshing.Dispose();

    private sealed record GraphUserPage
    {
        [JsonPropertyName("value")] public List<GraphUser>? Value { get; init; }

        [JsonPropertyName("@odata.nextLink")] public string? NextLink { get; init; }
    }

    private sealed record GraphUser
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("displayName")] public string? DisplayName { get; init; }
        [JsonPropertyName("jobTitle")] public string? JobTitle { get; init; }
        [JsonPropertyName("mail")] public string? Mail { get; init; }
        [JsonPropertyName("userPrincipalName")] public string? UserPrincipalName { get; init; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; init; }
    }
}

/// <summary>
/// Turning a directory entry into the row a visit's foreign key can point at.
///
/// vms.Person stops being an address list loaded from a spreadsheet and becomes a record of
/// the people who have actually been visited - written the first time somebody picks them,
/// and kept afterwards. That is what lets the visitor report keep working, and what keeps a
/// visit readable years later when the person has left the tenant and Graph no longer
/// returns them at all.
/// </summary>
public static class DirectoryPeople
{
    /// <summary>
    /// The local row for a directory entry, created if this is the first visit to them and
    /// brought up to date if the directory has since changed their title or company.
    ///
    /// Matched on the object ID, which is the only identifier Entra guarantees is stable -
    /// a display name is not unique and an address changes with a marriage or a rebrand.
    /// Falls back to matching an email address once, so the people already loaded from the
    /// export are recognised instead of being inserted a second time.
    ///
    /// Saves nothing itself. The caller is writing a visit in the same SaveChanges, and the
    /// two belong in one transaction: a host row with no visit is litter, and a visit whose
    /// host row was rolled back is a broken key.
    /// </summary>
    public static async Task<Person> EnsureAsync(VmsDbContext db, DirectoryPerson person, CancellationToken ct = default)
    {
        var existing = await db.People.FirstOrDefaultAsync(p => p.DirectoryObjectId == person.ObjectId, ct);

        if (existing is null && !string.IsNullOrWhiteSpace(person.Email))
        {
            existing = await db.People.FirstOrDefaultAsync(
                p => p.DirectoryObjectId == null && p.Email == person.Email, ct);

            // Claimed by the directory from here on, so the next visit matches on the ID.
            if (existing is not null) existing.DirectoryObjectId = person.ObjectId;
        }

        if (existing is null)
        {
            existing = new Person
            {
                DirectoryObjectId = person.ObjectId,
                DisplayName = person.DisplayName,
                Title = person.Title,
                Email = person.Email,
                CompanyName = person.CompanyName,
                IsActive = true,
            };

            db.People.Add(existing);
            return existing;
        }

        existing.DisplayName = person.DisplayName;
        existing.Title = person.Title;
        existing.Email = person.Email;
        existing.CompanyName = person.CompanyName;

        /* Deliberately not touched: IsActive and DiEntityId. Both are maintained against
           this table - IsActive by whoever retires a row, DiEntityId by the entity mapping
           in db/tools/generate_seed_people.py - and Graph has no equivalent of either that
           could be trusted to overwrite them. */
        return existing;
    }
}
