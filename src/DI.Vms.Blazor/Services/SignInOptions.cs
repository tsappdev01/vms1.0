using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Whether the application requires anyone to sign in.
///
/// A switch rather than two versions of the app. Entra ID is wired up and works, but the
/// directory work behind it - the API scope, the Android platform registration, the role
/// assignments - is not finished, and reception needs the desk working before it is. So
/// sign-in is off for now and turns on with one setting, with no rebuild and nothing to
/// re-implement.
///
/// It defaults to <c>false</c> deliberately: a deployment whose <c>AzureAd</c> section is
/// half-filled would otherwise lock every user out at the desk, which is a worse failure
/// than the one this setting is admitting to. The trade is that a server nobody
/// configured is open, so <see cref="LogTo"/> says so loudly at every startup and the
/// layout carries a banner on every page.
/// </summary>
public sealed class SignInOptions
{
    /// <summary>Require an Entra ID identity for every page and for the API.</summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// What goes in <c>RecordedBy</c> while sign-in is off.
    ///
    /// Not blank and not a real-looking name. Every entry recorded in this period is
    /// marked the same way, and the report shows it - so it is visible later which
    /// records have an author behind them and which only have a desk.
    /// </summary>
    public const string NotSignedIn = "(not signed in)";

    public static SignInOptions FromConfiguration(IConfiguration configuration) => new()
    {
        Enabled = configuration.GetValue("Authentication:Enabled", false),
    };

    public void LogTo(ILogger logger)
    {
        if (Enabled)
        {
            logger.LogInformation("Authentication is ON. Entra ID is required for every page and for the API.");
            return;
        }

        logger.LogWarning(
            "Authentication is OFF. Every page and every API endpoint is open to anyone who can reach this " +
            "server, and visits are recorded against \"{RecordedBy}\" rather than a person. Set " +
            "Authentication:Enabled to true in appsettings.Production.json once the Entra registration is " +
            "finished - see docs/entra-id-setup.md.",
            NotSignedIn);
    }
}

/// <summary>
/// The authentication scheme used while sign-in is off: everyone is the desk, and the desk
/// may do anything.
///
/// This exists so that turning sign-in off changes one thing and not many. Every
/// <c>[Authorize]</c> attribute, every policy and every <c>AuthorizeView</c> stays exactly
/// as written and keeps being evaluated - they simply all pass. The alternative, making
/// the policies permissive when disabled, would mean the authorisation rules that matter
/// are the ones never exercised until the day they are switched on.
/// </summary>
public sealed class OpenDeskAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "VmsOpenDesk";

    public OpenDeskAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, SignInOptions.NotSignedIn),

            /* The same claim Entra sends, and the first one VisitsApi looks for. Keeping
               the shape identical means the audit column reads the same way whether the
               record came from a browser or a tablet, signed in or not. */
            new("preferred_username", SignInOptions.NotSignedIn),
        };

        claims.AddRange(VmsRoles.All.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, ClaimTypes.Role);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
