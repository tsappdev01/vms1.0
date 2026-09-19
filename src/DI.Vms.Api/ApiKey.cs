using System.Security.Cryptography;
using System.Text;

namespace DI.Vms.Api;

/// <summary>
/// The credential the tablet presents while Entra sign-in is off.
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
/// without a redeploy. It is the interim until <c>Authentication:Enabled</c> goes true.
/// </summary>
public static class ApiKey
{
    public const string HeaderName = "X-Vms-Key";

    /// <summary>
    /// Refuses to start a host that has neither Entra nor a key.
    ///
    /// Deliberately a startup failure rather than a warning. A warning in a log nobody
    /// reads is how an unauthenticated visitor-records API ends up on the internet for a
    /// month, and the whole point of this class is that the unguarded case cannot happen
    /// by omission.
    /// </summary>
    public static string? Require(IConfiguration configuration, bool signInEnabled)
    {
        var key = configuration["Api:Key"];

        if (signInEnabled)
        {
            // Entra governs. A key may still be set and is simply not consulted.
            return string.IsNullOrWhiteSpace(key) ? null : key;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "This API has neither Entra ID sign-in nor an API key, and it is reachable from the " +
                "internet. Set Authentication:Enabled to true, or set Api:Key to a long random value " +
                "in the Web App's configuration and send it from the tablet as the " +
                $"{HeaderName} header. See docs/azure-deployment.md.");
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                $"Api:Key is {key.Length} characters. Use at least 32 - this is the only thing " +
                "standing in front of the visitor database while sign-in is off.");
        }

        return key;
    }

    /// <summary>
    /// Checks the header on every /api request, before authorisation runs.
    ///
    /// Only when there is no Entra: with a token the token is the credential, and asking
    /// for both would mean two things to rotate and no more safety.
    /// </summary>
    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app, string? expected)
    {
        if (expected is null) return app;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);

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
