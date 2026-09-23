using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// The credential the tablet presents instead of signing in.
///
/// This exists because of where this host lives. The Blazor app sits on UATWEB01, inside
/// the office network, and running it with sign-in off means it is open to people who are
/// already on that network. This one is on the public internet, and "open" there means
/// open to everybody - an endpoint that writes Emirates ID numbers and photographs into
/// the visitor database, available to anyone who finds the hostname.
///
/// So a key is required whenever Entra is not. It is not as good as a token: it is the
/// same secret on every tablet, it does not expire, it identifies a fleet rather than a
/// person, and anyone who unpacks the APK can read it. What it does buy is that the
/// endpoint is not answering strangers, and that the key can be rotated in the portal
/// without a redeploy.
///
/// It is not an interim any more. Sign-in on the web app and a key on the tablet are the
/// end state, because a reception tablet sits on a counter and is handed to nobody: it is
/// not a person's device and cannot hold a person's credential. So with
/// <c>Authentication:Enabled</c> true the key becomes an authentication scheme beside the
/// bearer token - see <see cref="ApiKeyAuthenticationHandler"/> - rather than being
/// ignored.
/// </summary>
public static class ApiKey
{
    public const string HeaderName = "X-Vms-Key";

    /// <summary>
    /// The key if one is configured, null if not. Enforced only when present.
    ///
    /// This is what the on-premises host uses. UATWEB01 is inside the office network and
    /// has run with no key since before one existed; demanding one there would break a
    /// working deployment to protect it from a threat it does not have.
    /// </summary>
    public static string? Configured(IConfiguration configuration)
    {
        var key = configuration["Api:Key"];
        if (string.IsNullOrWhiteSpace(key)) return null;

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                $"Api:Key is {key.Length} characters. Use at least 32 - on a public host this is the " +
                "only thing standing in front of the visitor database while sign-in is off.");
        }

        return key;
    }

    /// <summary>
    /// The same, but refuses to start when there is neither Entra nor a key.
    ///
    /// Used by the host that exists to be reachable from the internet. Deliberately a
    /// startup failure rather than a warning: a warning in a log nobody reads is how an
    /// unauthenticated visitor-records API stays online for a month, and the point of this
    /// class is that the unguarded case cannot happen by omission.
    /// </summary>
    public static string? RequireForPublicHost(IConfiguration configuration, bool signInEnabled)
    {
        var key = Configured(configuration);

        /* With Entra on, the browser has a credential whatever this returns - so a host
           with no key still starts. It just has no way for a tablet to reach it. */
        if (signInEnabled) return key;

        if (key is null)
        {
            throw new InvalidOperationException(
                "This host has neither Entra ID sign-in nor an API key, and it is reachable from the " +
                "internet. Set Authentication:Enabled to true, or set Api:Key to a long random value " +
                "in the Web App's configuration and send it from the tablet as the " +
                $"{HeaderName} header. See docs/azure-deployment.md.");
        }

        return key;
    }

    /// <summary>
    /// Checks the header on every /api request, before authorisation runs.
    ///
    /// Only used while sign-in is off, where there is no authentication stack to hang the
    /// key off and a flat rejection is the whole of the rule. With sign-in on the key is an
    /// authentication scheme instead, so that the same
    /// <see cref="VmsRoles.CanCheckIn"/> policy decides either way and the key is a way of
    /// arriving rather than a way around the rules.
    /// </summary>
    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app, string? expected)
    {
        if (expected is null) return app;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ApiKey));

        return app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await next();
                return;
            }

            var presented = context.Request.Headers[HeaderName].ToString();

            /* Fixed-time, and length-checked first because FixedTimeEquals requires equal
               lengths. Comparing with == would leak the key a character at a time to
               anyone patient enough to measure. */
            var presentedBytes = Encoding.UTF8.GetBytes(presented);

            if (presentedBytes.Length != expectedBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(presentedBytes, expectedBytes))
            {
                /* Logged with the caller's address, because on a public host this is the
                   signal that someone is trying keys. One line per attempt is what makes a
                   pattern visible; the key itself is never logged, not even a prefix. */
                logger.LogWarning(
                    "Rejected {Method} {Path} from {Address}: {Reason} {Header}.",
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress?.ToString() ?? "(unknown)",
                    presentedBytes.Length == 0 ? "no" : "wrong",
                    HeaderName);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Not authorised",
                    detail = $"This tablet did not present a valid {HeaderName}.",
                    status = 401,
                });
                return;
            }

            await next();
        });
    }
}

/// <summary>Carries the key the handler compares against. One per host.</summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ExpectedKey { get; set; } = string.Empty;
}

/// <summary>
/// The tablet's key, as an authentication scheme, for a host where sign-in is on.
///
/// A scheme rather than a second piece of middleware, because that is what lets the
/// endpoints keep one rule. <c>/api</c> names both schemes and the
/// <see cref="VmsRoles.CanCheckIn"/> policy; whichever of the two authenticated, the same
/// policy then decides. Nothing is bypassed and there is no branch in the endpoint saying
/// "unless it came from a tablet".
///
/// The identity it produces is deliberately thin. One role - <see cref="VmsRoles.Officer"/>
/// - so a key can check a visitor in and nothing else: not the report, not an unmasked ID
/// number. And the name is <see cref="SignInOptions.NotSignedIn"/>, the same string the
/// open-desk handler uses, so <c>RecordedBy</c> says what is true. A tablet is a desk, not
/// a person.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string SchemeName = "VmsTabletKey";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var presented = Request.Headers[ApiKey.HeaderName].ToString();

        /* No header is not a failure. It means this request is somebody else's - the desk
           browser's bearer token - and failing here would turn every signed-in call into a
           401 from the wrong handler. */
        if (string.IsNullOrEmpty(presented)) return Task.FromResult(AuthenticateResult.NoResult());

        var presentedBytes = Encoding.UTF8.GetBytes(presented);
        var expectedBytes = Encoding.UTF8.GetBytes(Options.ExpectedKey);

        /* Fixed-time, and length-checked first because FixedTimeEquals requires equal
           lengths. Comparing with == would leak the key a character at a time to anyone
           patient enough to measure. */
        if (presentedBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(presentedBytes, expectedBytes))
        {
            /* Logged with the caller's address, because on a public host this is the signal
               that someone is trying keys. One line per attempt is what makes a pattern
               visible; the key itself is never logged, not even a prefix. */
            Logger.LogWarning(
                "Rejected {Method} {Path} from {Address}: wrong {Header}.",
                Request.Method,
                Request.Path,
                Context.Connection.RemoteIpAddress?.ToString() ?? "(unknown)",
                ApiKey.HeaderName);

            return Task.FromResult(AuthenticateResult.Fail($"The {ApiKey.HeaderName} presented is not valid."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, SignInOptions.NotSignedIn),

            /* The same claim Entra sends, and the first one VisitsApi looks for. Keeping the
               shape identical means the audit column reads the same way whether the record
               came from a signed-in browser or from a tablet. */
            new("preferred_username", SignInOptions.NotSignedIn),

            new(ClaimTypes.Role, VmsRoles.Officer),
        };

        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, ClaimTypes.Role);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
