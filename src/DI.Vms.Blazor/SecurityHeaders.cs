using DI.Vms.Blazor.Services;

namespace DI.Vms.Blazor;

/// <summary>
/// The response headers a browser is told to enforce, and the cookie rules that go with them.
///
/// Required by DI-IT-POL-AIDEV-001 §8.4 for a Controlled application. Until now the only one
/// sent was HSTS, plus a Content-Security-Policy on the card-image endpoint alone - which was
/// the one place it had been needed to make an SVG safe to render, and nowhere else.
///
/// <b>The policy is built from this deployment's own configuration, not written as a
/// constant.</b> That is the whole difficulty of a CSP on this application: two of the things
/// it does legitimately look like the things a CSP exists to stop.
///
/// The digital-card scanner fetches Tesseract and its language data from wherever
/// <see cref="DigitalCardOptions"/> points - a public CDN by default - and runs it as
/// WebAssembly in a worker. The desk's card reader is reached by the browser opening a
/// WebSocket to ICP's agent on the attendant's own machine. A policy written from a template
/// blocks both, and the symptom is a scanner that silently never starts and a reader that is
/// never found - neither of which looks like a header problem to the person at the desk.
///
/// So the sources are derived from the same options the features themselves use. Turn the
/// scanner off and its origins leave the policy; point it at <c>/lib/tesseract</c> and they
/// leave as well, because it is then same-origin.
/// </summary>
public static class SecurityHeaders
{
    /// <summary>
    /// Adds the headers to every response, and hardens the authentication cookie.
    ///
    /// Call before the endpoints so that it covers static files and the Blazor circuit as
    /// well as the pages; the headers are set on the way in, so they are present whatever the
    /// endpoint does afterwards.
    /// </summary>
    public static IApplicationBuilder UseVmsSecurityHeaders(
        this IApplicationBuilder app,
        DigitalCardOptions digitalCard,
        CardCaptureOptions capture,
        bool httpsAvailable)
    {
        var policy = ContentSecurityPolicy(digitalCard, capture, httpsAvailable);

        return app.Use(async (http, next) =>
        {
            var headers = http.Response.Headers;

            /*  Not overwritten where something has already set one.
             *
             *  The card-image endpoint sends a far stricter policy of its own - the response
             *  is an SVG, which is a document that can carry script - and that one must win.
             *  It sets its header before the body is written, which is after this runs. */
            headers.TryAdd("Content-Security-Policy", policy);

            /* Clickjacking. frame-ancestors in the policy above is the modern control and
               covers browsers that read it; this is for the ones that do not. */
            headers.TryAdd("X-Frame-Options", "DENY");

            // No MIME sniffing: a file served as text must not be executed as script.
            headers.TryAdd("X-Content-Type-Options", "nosniff");

            /* Same-origin only, so a visitor's record ID never travels in a Referer to
               anywhere else. strict-origin-when-cross-origin would still send the origin. */
            headers.TryAdd("Referrer-Policy", "same-origin");

            /*  The camera is allowed, and only for this origin.
             *
             *  Scanning a card is the one capability this application legitimately needs, so
             *  it is the one that is not disabled. Everything else a browser might offer is
             *  switched off rather than left to default, which is what the policy asks for.
             */
            headers.TryAdd(
                "Permissions-Policy",
                "camera=(self), microphone=(), geolocation=(), payment=(), usb=(), " +
                "accelerometer=(), gyroscope=(), magnetometer=(), interest-cohort=()");

            await next();
        });
    }

    /// <summary>
    /// The policy, assembled from what this deployment actually loads.
    ///
    /// <c>'unsafe-inline'</c> for styles is deliberate and is not laxity: Blazor writes
    /// element styles directly, and the alternative is a nonce on every one of them, which
    /// this application does not have the machinery for. Script has no such allowance.
    /// </summary>
    private static string ContentSecurityPolicy(
        DigitalCardOptions digitalCard,
        CardCaptureOptions capture,
        bool httpsAvailable)
    {
        var script = new List<string> { "'self'" };
        var connect = new List<string> { "'self'" };
        var worker = new List<string> { "'self'", "blob:" };

        /*  Tesseract, where it is served from somewhere else.
         *
         *  The engine is fetched and then run as WebAssembly, which needs wasm-unsafe-eval -
         *  without it the scanner throws on the first frame and the screen says only that the
         *  recogniser would not start. The worker it spawns comes from a blob: URL, which is
         *  why blob: is in worker-src above whether or not a CDN is used.
         */
        if (digitalCard.Enabled)
        {
            script.Add("'wasm-unsafe-eval'");

            foreach (var origin in Origins(digitalCard.EngineBaseUrl, digitalCard.TrainedDataUrl))
            {
                script.Add(origin);
                connect.Add(origin);
            }
        }

        /*  ICP's agent, which the browser talks to over a WebSocket.
         *
         *  In agent mode the reader is on the attendant's machine and only the browser can
         *  reach it, so connect-src has to allow it. The host and the scheme come from the
         *  same options the agent code uses, so a deployment that moves the agent does not
         *  also have to remember to edit a header.
         */
        if (capture.Mode == CardCaptureMode.Agent)
        {
            var host = capture.Agent.HostName is { Length: > 0 } named
                ? named
                : capture.Agent.TlsEnabled ? "toolkitagent.emiratesid.ae" : "127.0.0.1";

            connect.Add(capture.Agent.TlsEnabled ? $"wss://{host}:*" : $"ws://{host}:*");

            // ICP's own script is served from this application, but it loads over the same
            // socket; the origin is needed for the handshake request itself.
            connect.Add(capture.Agent.TlsEnabled ? $"https://{host}:*" : $"http://{host}:*");
        }

        var directives = new List<string>
        {
            "default-src 'self'",
            $"script-src {string.Join(' ', script.Distinct())}",

            // Blazor sets element styles directly; see the remark above.
            "style-src 'self' 'unsafe-inline'",

            // data: for the card photograph, which is rendered from bytes rather than a URL.
            "img-src 'self' data:",

            "font-src 'self'",
            $"connect-src {string.Join(' ', connect.Distinct())}",
            $"worker-src {string.Join(' ', worker.Distinct())}",

            // Nothing on these screens is a plugin, a frame, or a form posting elsewhere.
            "object-src 'none'",
            "frame-ancestors 'none'",
            "base-uri 'self'",
            "form-action 'self'",
        };

        /* Only where HTTPS exists. On the reception-PC deployment, which binds plain HTTP on
           127.0.0.1 and has no certificate, this would upgrade every request to a port
           nothing is listening on. */
        if (httpsAvailable) directives.Add("upgrade-insecure-requests");

        return string.Join("; ", directives);
    }

    /// <summary>
    /// The scheme-and-host of each absolute URL, ignoring anything relative.
    ///
    /// A relative path is same-origin and already covered by <c>'self'</c>, so a deployment
    /// that hosts Tesseract under wwwroot adds nothing to the policy - which is the quiet
    /// argument for hosting it there.
    /// </summary>
    private static IEnumerable<string> Origins(params string?[] urls)
    {
        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url)) continue;

            if (Uri.TryCreate(url, UriKind.Absolute, out var parsed) &&
                parsed.Scheme is "http" or "https")
            {
                yield return $"{parsed.Scheme}://{parsed.Host}";
            }
        }
    }
}
