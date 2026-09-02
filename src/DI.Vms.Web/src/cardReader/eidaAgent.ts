import type { CardReadResult, CardStatus, ICardReader, ReaderState } from './types';

/* Adapter over the ICP toolkit agent installed by ICAToolkitService.msi.
   The browser talks to it over a WebSocket on the local machine; eidatoolkit.js
   from the Windows SDK provides the call surface and must be loaded on the page.

   This is written against the agent's documented API but has NOT been exercised
   against a live agent or a real card - no reader has been attached yet. Treat the
   first run as a spike, not as working code. */

declare global {
  interface Window {
    /* eidatoolkit.js attaches its entry points here. Typed loosely on purpose:
       the exact surface must be confirmed against the agent before this is trusted. */
    service_context?: unknown;
    initialize?: (...args: unknown[]) => void;
    listReaders?: (...args: unknown[]) => void;
    readPublicData?: (...args: unknown[]) => void;
    isCardGenuine?: (...args: unknown[]) => void;
    checkCardStatus?: (...args: unknown[]) => void;
    probeHealth?: (timeoutMs?: number) => Promise<{ tls: boolean; host: string } | null>;
  }
}

const HEALTH_URL = 'http://127.0.0.1:9006/health';
const HEALTH_TIMEOUT_MS = 2000;

export class EidaAgentCardReader implements ICardReader {
  async probe(): Promise<ReaderState> {
    const unavailable = (detail: string): ReaderState => ({
      available: false, readerName: null, detail, simulated: false,
    });

    // 1. Is the local agent running at all?
    let healthy = false;
    try {
      const controller = new AbortController();
      const timer = setTimeout(() => controller.abort(), HEALTH_TIMEOUT_MS);
      const res = await fetch(HEALTH_URL, { signal: controller.signal });
      clearTimeout(timer);
      healthy = res.ok;
    } catch {
      healthy = false;
    }

    if (!healthy) {
      return unavailable(
        'The ID Card Toolkit agent is not responding on this machine. ' +
        'Install ICAToolkitService.msi from the Windows SDK and confirm the service is running.');
    }

    // 2. Has eidatoolkit.js been loaded onto the page?
    if (typeof window.listReaders !== 'function') {
      return unavailable(
        'eidatoolkit.js is not loaded. Add it to the page before using a physical reader.');
    }

    return {
      available: true,
      readerName: null, // filled in once listReaders has been wired to its callback
      detail: 'Toolkit agent reachable.',
      simulated: false,
    };
  }

  async read(): Promise<CardReadResult> {
    /* Intentionally not implemented. The agent's calls are callback-based and their
       exact request/response envelopes must be confirmed against a live agent and a
       real card. Guessing them would produce code that looks finished and is not. */
    throw new Error(
      'The physical reader path is not wired up yet. It needs one session against the ' +
      'toolkit agent with a real Emirates ID to confirm the call envelopes. ' +
      'Use the simulator until then.');
  }
}

/** Maps the toolkit's card status codes onto the local vocabulary. */
export function toCardStatus(raw: string | number | null | undefined): CardStatus {
  const value = String(raw ?? '').toLowerCase();
  if (value.includes('valid') || value === '0') return 'Valid';
  if (value.includes('expire')) return 'Expired';
  if (value.includes('lost')) return 'Lost';
  if (value.includes('stolen')) return 'Stolen';
  if (value.includes('revoke')) return 'Revoked';
  return 'Unknown';
}
