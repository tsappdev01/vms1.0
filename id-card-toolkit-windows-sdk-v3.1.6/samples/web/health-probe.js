/* ============================================================================
 * Probe http://127.0.0.1:9006/health and return SDK options derived from
 * the agent's response. ES5 callback style; returns a Promise.
 *
 * Used by samples/web/modern/index.html and samples/web/console/index.html
 * to skip the hardcoded ws/wss guess when the agent's /health endpoint is
 * reachable. See docs/superpowers/specs/2026-05-10-modern-console-samples-
 * health-probe-design.md for the full design.
 *
 * Exposes one function on window:
 *   probeHealth(timeoutMs)  -> Promise<{tls: boolean, host: string} | null>
 *
 * Resolves to null on any failure (timeout / network error / non-200 /
 * malformed JSON / missing agent_tls_enabled). Never rejects -- callers
 * branch on truthy/null only.
 * ========================================================================== */

(function (root) {
    'use strict';

    var DEFAULT_HOST_TLS    = 'toolkitagent.emiratesid.ae';
    var DEFAULT_HOST_NONTLS = '127.0.0.1';
    var PROBE_URL           = 'http://127.0.0.1:9006/health';
    var DEFAULT_TIMEOUT_MS  = 2000;

    function probeHealth(timeoutMs) {
        var to = (typeof timeoutMs === 'number') ? timeoutMs : DEFAULT_TIMEOUT_MS;

        return new Promise(function (resolve) {
            var ctrl = (typeof AbortController !== 'undefined')
                ? new AbortController() : null;
            var resolved = false;
            function done(value) {
                if (resolved) { return; }
                resolved = true;
                clearTimeout(timer);
                resolve(value);
            }
            var timer = setTimeout(function () {
                if (ctrl) { ctrl.abort(); }
                done(null);
            }, to);

            fetch(PROBE_URL, {
                method: 'GET',
                cache: 'no-store',
                signal: ctrl ? ctrl.signal : undefined
            }).then(function (resp) {
                if (!resp.ok) { done(null); return null; }
                return resp.json();
            }).then(function (body) {
                if (resolved) { return; }
                if (!body || typeof body.agent_tls_enabled !== 'boolean') {
                    done(null);
                    return;
                }
                var tls = body.agent_tls_enabled;
                var host = (body.main_listener && body.main_listener.host)
                    || (tls ? DEFAULT_HOST_TLS : DEFAULT_HOST_NONTLS);
                done({ tls: tls, host: host });
            })['catch'](function () {
                done(null);
            });
        });
    }

    root.probeHealth = probeHealth;

}(typeof window !== 'undefined' ? window : this));
