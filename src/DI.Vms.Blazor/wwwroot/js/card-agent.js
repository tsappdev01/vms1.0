/**
 * Reads an Emirates ID through the ICP agent running on the reception desk.
 *
 * The app is on a server, so the server has no reader. eidatoolkit.js - ICP's file, in
 * ../lib/eidatoolkit/ - opens a WebSocket to an agent on the desk's own machine and the
 * agent drives the reader. This module is the promise-shaped wrapper the Blazor component
 * calls, and the only part of the browser side that is ours.
 *
 * Two things it deliberately does NOT do:
 *
 * 1. It does not parse the card. Every field the desk sees comes back out of the signed
 *    XML on the server (Services/AgentCardReader.cs). Anything parsed here would be a
 *    field the server took on trust from a browser, which is the whole risk of moving the
 *    read off the server in the first place.
 *
 * 2. It does not let the SDK navigate away. When no agent answers, eidatoolkit.js puts up
 *    a confirm() and sets window.location to a JNLP download - which would take the
 *    attendant out of the app mid-check-in, and with no jnlp_address configured would
 *    navigate to "undefined". So the agent is probed first, here, and the SDK is only
 *    constructed once something is known to be listening.
 */

/* Ports in the order eidatoolkit.js tries them, so a probe failure means the SDK would
   have failed too. */
const AGENT_PORTS = [9004, 9005, 9020];
const PROBE_TIMEOUT_MS = 1500;

/* The SDK's own defaults, which depend on the scheme: under TLS the name resolves to the
   loopback address, which is how the agent can hold a certificate a browser will accept
   for something that is nonetheless on this desk. */
const DEFAULT_HOST_TLS = 'toolkitagent.emiratesid.ae';
const DEFAULT_HOST_PLAIN = '127.0.0.1';

let sdkPromise = null;

/**
 * Loads ICP's script on demand and caches the attempt.
 *
 * On demand because it is 120 KB that the report screen has no use for, and injected
 * rather than declared in the layout so the load order cannot depend on where the
 * attendant happened to land first.
 */
function loadSdk() {
    if (sdkPromise) { return sdkPromise; }

    sdkPromise = new Promise((resolve, reject) => {
        if (typeof window.Toolkit === 'function') { resolve(); return; }

        const script = document.createElement('script');
        script.src = 'lib/eidatoolkit/eidatoolkit.js';
        script.async = true;
        script.onload = () => {
            typeof window.Toolkit === 'function'
                ? resolve()
                : reject(new Error('eidatoolkit.js loaded but defines no Toolkit.'));
        };
        script.onerror = () => reject(new Error('eidatoolkit.js could not be loaded from the server.'));
        document.head.appendChild(script);
    });

    /* A failed load must not be cached, or one bad network moment disables card reading
       until the page is reloaded. */
    sdkPromise.catch(() => { sdkPromise = null; });
    return sdkPromise;
}

function host(options) {
    if (options.hostName) { return options.hostName; }
    return options.tlsEnabled ? DEFAULT_HOST_TLS : DEFAULT_HOST_PLAIN;
}

/**
 * Is anything listening where the agent should be? Resolves to the port that answered, or
 * null.
 *
 * A WebSocket handshake is the only probe available: fetch() cannot see a ws:// endpoint,
 * and the browser gives no reason for a failed cross-origin connection. So "no" here
 * means "no agent, wrong port, or its certificate is not trusted on this desk" - which is
 * why the message the component shows names all three.
 */
async function probePort(options, port) {
    const url = `${options.tlsEnabled ? 'wss' : 'ws'}://${host(options)}:${port}`;

    return new Promise(resolve => {
        let socket;
        const finish = answered => {
            clearTimeout(timer);
            if (socket) {
                socket.onopen = socket.onerror = socket.onclose = null;
                /* Closed immediately either way: the probe must not leave a socket open
                   that the SDK would then find already active. */
                try { socket.close(); } catch { /* already closing */ }
            }
            resolve(answered ? port : null);
        };

        const timer = setTimeout(() => finish(false), PROBE_TIMEOUT_MS);

        try {
            // 'eida-toolkit' is the subprotocol the agent speaks; the SDK uses the same.
            socket = new WebSocket(url, 'eida-toolkit');
        } catch {
            finish(false);
            return;
        }

        socket.onopen = () => finish(true);
        socket.onerror = () => finish(false);
        socket.onclose = () => finish(false);
    });
}

async function findAgent(options) {
    for (const port of AGENT_PORTS) {
        const answered = await probePort(options, port);
        if (answered !== null) { return answered; }
    }
    return null;
}

/**
 * Brings up a Toolkit and resolves once the agent has established its context - which is
 * when the SDK's onOpen fires, not when the socket opens.
 */
function connect(options) {
    return new Promise((resolve, reject) => {
        let settled = false;
        const settle = fn => { if (!settled) { settled = true; fn(); } };

        let toolkit;
        const fail = detail => settle(() => reject(new Error(String(detail ?? 'The agent reported an error.'))));

        try {
            toolkit = new window.Toolkit(
                () => settle(() => resolve(toolkit)),
                code => fail(`The agent closed the connection (code ${code}).`),
                error => fail(error && error.message ? error.message : error),
                {
                    /* Our own page, not a JNLP. The agent is probed before this is
                       reached, so it should never fire - but if the SDK ever does decide
                       to navigate, it will land somewhere that explains itself rather
                       than downloading Java. */
                    jnlp_address: 'agent-required',
                    debugEnabled: !!options.debugEnabled,
                    agent_tls_enabled: !!options.tlsEnabled,
                    agent_host_name: options.hostName || '',
                    toolkitConfig: options.toolkitConfig || '',
                });
        } catch (error) {
            fail(error && error.message ? error.message : error);
        }
    });
}

/** The SDK's callbacks are (result, error); this is the promise around one call. */
function call(receiver, method, args) {
    return new Promise((resolve, reject) => {
        try {
            receiver[method](...args, (result, error) => {
                error ? reject(new Error(message(error))) : resolve(result);
            });
        } catch (error) {
            reject(new Error(message(error)));
        }
    });
}

function message(error) {
    if (!error) { return 'The agent reported an error with no detail.'; }
    if (typeof error === 'string') { return error; }
    return error.message || error.error_message || String(error);
}

function release(toolkit) {
    try { toolkit && toolkit.cleanup(); } catch { /* going away anyway */ }
}

/**
 * What the desk needs to know before anyone puts a card in: is the agent there, what is
 * its licence, and is a reader attached.
 *
 * Never throws - every failure is a state the screen has to describe.
 */
export async function probe(options) {
    try {
        await loadSdk();
    } catch (error) {
        return { agentAvailable: false, detail: message(error) };
    }

    const port = await findAgent(options);
    if (port === null) {
        return {
            agentAvailable: false,
            detail: `No ICP agent answered on ${host(options)} (ports ${AGENT_PORTS.join(', ')}). ` +
                'Install the ICP agent on this PC, or - if it is installed - check that it is running ' +
                'and that its certificate is trusted here.',
        };
    }

    let toolkit = null;
    try {
        toolkit = await connect(options);

        const result = { agentAvailable: true, agentPort: port };

        /* Each is asked for on its own: a desk with no card in the reader must still be
           able to report its toolkit version and licence, because that is exactly the
           state in which someone is trying to work out what is wrong. */
        try { result.toolkitVersion = await call(toolkit, 'getToolkitVersion', []); } catch { /* reported as absent */ }
        try { result.licenceExpiry = await call(toolkit, 'getLicenseExpiryDate', []); } catch { /* ditto */ }

        try {
            const reader = await call(toolkit, 'getReaderWithEmiratesId', []);
            result.readerName = reader.getReaderName();
            result.cardPresent = true;
            result.detail = 'Card detected. Ready to read.';
        } catch (error) {
            result.cardPresent = false;
            result.detail = message(error);
        }

        return result;
    } catch (error) {
        return { agentAvailable: false, agentPort: port, detail: message(error) };
    } finally {
        release(toolkit);
    }
}

/**
 * Reads the card and returns the signed response, and nothing but the signed response.
 *
 * requestId is issued by the server and stamped into the response by the gateway, which
 * is what stops this call being replayed with a document from an earlier visitor.
 */
export async function read(options, requestId) {
    await loadSdk();

    const port = await findAgent(options);
    if (port === null) {
        throw new Error('No ICP agent is running on this PC, so the card cannot be read here.');
    }

    let toolkit = null;
    try {
        toolkit = await connect(options);

        const reader = await call(toolkit, 'getReaderWithEmiratesId', []);
        await call(reader, 'connect', []);

        try {
            /* Non-modifiable data, the photograph, the card's signature image and the
               address. Modifiable data is not read: occupation, sponsor and passport
               details are not what visitor management is for. The argument order is the
               SDK's: (requestId, nonModifiable, modifiable, photo, signature, address). */
            const response = await call(reader, 'readPublicData', [requestId, true, false, true, true, true]);

            if (!response || !response.xmlString) {
                throw new Error('The agent returned no signed response for the card.');
            }

            return { xml: response.xmlString };
        } finally {
            try { await call(reader, 'disconnect', []); } catch { /* the card may already be out */ }
        }
    } finally {
        release(toolkit);
    }
}
