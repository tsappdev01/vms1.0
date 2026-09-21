namespace DI.Vms.Blazor.Services;

/// <summary>Where the "person to visit" list comes from.</summary>
public enum DirectorySource
{
    /// <summary>vms.Person - the AD export loaded by db/004_seed_people.sql.</summary>
    Database,

    /// <summary>Entra ID, live, through Microsoft Graph. The same directory people sign in with.</summary>
    EntraId,
}

/// <summary>
/// How the host list is looked up.
///
/// The export in vms.Person was a snapshot of the directory taken on 3 July. It ages from
/// the day it is loaded: a joiner is not in it, a leaver still is, and a change of title or
/// company needs a new export and a new run of the generator. Reading Entra ID directly
/// removes that whole class of staleness - the list at the desk is the list in the tenant.
///
/// Graph is called with the application's own identity (client credentials), not the
/// receptionist's. Two reasons: sign-in can be off, and the desk should not need a signed-in
/// user to look up a host; and the permission wanted is read-only over the directory, which
/// is cleaner to grant and to audit once for the app than delegated per user. It needs
/// User.Read.All as an *application* permission with admin consent - see
/// docs/entra-id-setup.md.
///
/// The credentials fall back to the AzureAd section, because the sign-in registration
/// already has a client secret and a second one would be a second thing to rotate. They can
/// still be set separately when the directory read should be its own registration.
/// </summary>
public sealed class DirectoryOptions
{
    public const string SectionName = "Directory";

    public DirectorySource Source { get; init; } = DirectorySource.Database;

    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>The login authority. Sovereign clouds differ; the default is the public one.</summary>
    public string Instance { get; init; } = "https://login.microsoftonline.com/";

    /// <summary>The Graph endpoint. Also cloud-specific, and also rarely changed.</summary>
    public string GraphBaseUrl { get; init; } = "https://graph.microsoft.com";

    /// <summary>
    /// How long a fetched copy of the directory is used before it is fetched again.
    ///
    /// The whole directory is held in memory and searched there, rather than one Graph
    /// query per keystroke: a type-ahead makes a request every few characters, and at a
    /// busy desk that is a request per visitor per letter against a tenant-wide throttle.
    /// Twenty minutes is a compromise between that and a joiner appearing promptly.
    /// </summary>
    public int CacheMinutes { get; init; } = 20;

    /// <summary>
    /// A ceiling on how many users are held. Not a limit anyone here should meet - the
    /// group is in the low thousands - but an unbounded fetch into memory on a single
    /// App Service instance is not something to leave to the size of a tenant.
    /// </summary>
    public int MaximumUsers { get; init; } = 20000;

    /// <summary>
    /// Suggestions shown for a typed search. More than a dozen matches on a name means the
    /// name was the wrong thing to narrow by, and a longer list does not help.
    /// </summary>
    public int MaximumSuggestions { get; init; } = 12;

    /// <summary>
    /// How many are listed when nothing has been typed - the directory being browsed rather
    /// than searched. Higher, because here the list is the point: the attendant is scrolling
    /// for a name they half remember. Not the whole tenant, though. Every entry is a
    /// component rendered over the circuit, and a thousand of them on focus is a stall at
    /// the desk to show names nobody will scroll to; past a screen or two, typing is faster.
    /// </summary>
    public int MaximumListed { get; init; } = 50;

    /// <summary>
    /// True when Graph could actually be called. Configuration alone does not prove the
    /// permission was granted - that shows up on the first call - but it does tell a
    /// missing setting from a failing one.
    /// </summary>
    public bool CredentialsPresent =>
        !string.IsNullOrWhiteSpace(TenantId)
        && !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret);

    public bool UsesEntraId => Source == DirectorySource.EntraId;

    public static DirectoryOptions FromConfiguration(IConfiguration config)
    {
        var section = config.GetSection(SectionName);
        var azureAd = config.GetSection("AzureAd");

        string Setting(string name, string fallback = "") =>
            section[name] is { Length: > 0 } value ? value : fallback;

        var tenantId = Setting("TenantId", azureAd["TenantId"] ?? string.Empty);
        var clientId = Setting("ClientId", azureAd["ClientId"] ?? string.Empty);
        var clientSecret = Setting("ClientSecret", azureAd["ClientSecret"] ?? string.Empty);

        /* Unset means "use Entra ID if it could work". The point of this change is that
           the host list is the sign-in directory, so that is what an unconfigured Source
           should mean - and on a machine with no credentials it still falls back to the
           table rather than starting an app whose picker can only fail. Set
           Directory:Source explicitly to override either way. */
        var source = section["Source"] is { Length: > 0 } text
            ? Enum.TryParse<DirectorySource>(text, ignoreCase: true, out var parsed)
                ? parsed
                : throw new InvalidOperationException(
                    $"Directory:Source is \"{text}\". It must be Database or EntraId.")
            : (!string.IsNullOrWhiteSpace(tenantId)
               && !string.IsNullOrWhiteSpace(clientId)
               && !string.IsNullOrWhiteSpace(clientSecret))
                ? DirectorySource.EntraId
                : DirectorySource.Database;

        return new DirectoryOptions
        {
            Source = source,
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Instance = Setting("Instance", azureAd["Instance"] ?? "https://login.microsoftonline.com/"),
            GraphBaseUrl = Setting("GraphBaseUrl", "https://graph.microsoft.com"),
            CacheMinutes = section.GetValue("CacheMinutes", 20),
            MaximumUsers = section.GetValue("MaximumUsers", 20000),
            MaximumSuggestions = section.GetValue("MaximumSuggestions", 12),
            MaximumListed = section.GetValue("MaximumListed", 50),
        };
    }
}
