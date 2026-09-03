import type { CardReadResult, CardStatus, ICardReader, ReaderState } from './types';

/* Talks to DI.Vms.CardBridge, a small process on the reception machine that owns the
   ID Card Toolkit and exposes it on loopback.

   The alternative was the ICP JavaScript agent over WebSocket. This route was chosen
   because it reuses the exact .NET path already proven to read a card on this hardware,
   rather than a second integration whose call envelopes are still unconfirmed. */

const BRIDGE = 'http://127.0.0.1:9100';
const TIMEOUT_MS = 20_000;   // a chip read plus two gateway calls is not instant

interface BridgeStatus {
  available: boolean;
  readerName: string | null;
  detail: string;
  toolkitVersion?: string;
  licenseExpiry?: string;
}

interface BridgeRead {
  idNumber: string;
  cardNumber: string | null;
  name: string | null;
  nationality: string | null;
  dateOfBirth: string | null;
  expiryDate: string | null;
  gender: string | null;
  photoBase64: string | null;
  verification: {
    isGenuine: boolean | null;
    cardStatus: string;
    vgAvailable: boolean;
    verifiedAtUtc: string | null;
  };
}

async function call<T>(path: string, init?: RequestInit): Promise<T> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);
  try {
    const res = await fetch(`${BRIDGE}${path}`, { ...init, signal: controller.signal });
    const body = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(body?.error ?? `${res.status} ${res.statusText}`);
    return body as T;
  } finally {
    clearTimeout(timer);
  }
}

/**
 * The toolkit reports dates as DD/MM/YYYY. The API expects ISO, and misreading one for
 * the other silently shifts a date rather than failing, so conversion is explicit.
 */
function toIso(value: string | null): string | null {
  if (!value) return null;
  const m = /^(\d{2})[/-](\d{2})[/-](\d{4})$/.exec(value.trim());
  if (m) return `${m[3]}-${m[2]}-${m[1]}`;
  return /^\d{4}-\d{2}-\d{2}/.test(value) ? value.slice(0, 10) : null;
}

function toCardStatus(raw: string | null | undefined): CardStatus {
  const v = String(raw ?? '').toLowerCase();
  if (v.includes('valid') || v === '0') return 'Valid';
  if (v.includes('expire')) return 'Expired';
  if (v.includes('lost')) return 'Lost';
  if (v.includes('stolen')) return 'Stolen';
  if (v.includes('revoke')) return 'Revoked';
  return 'Unknown';
}

export class BridgeCardReader implements ICardReader {
  async probe(): Promise<ReaderState> {
    try {
      const status = await call<BridgeStatus>('/reader/status');
      return {
        available: status.available,
        readerName: status.readerName,
        detail: status.available
          ? `${status.readerName ?? 'Reader'} ready.`
          : status.detail,
        simulated: false,
      };
    } catch {
      return {
        available: false,
        readerName: null,
        detail:
          'The card bridge is not running on this machine. Start DI.Vms.CardBridge.exe ' +
          'on the reception PC, then reload this page.',
        simulated: false,
      };
    }
  }

  async read(): Promise<CardReadResult> {
    const r = await call<BridgeRead>('/reader/read', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ photo: true }),
    });

    return {
      data: {
        idNumber: r.idNumber,
        name: r.name ?? '',
        nationality: r.nationality,
        dateOfBirth: toIso(r.dateOfBirth),
        expiryDate: toIso(r.expiryDate),
        gender: r.gender,
        photoBase64: r.photoBase64,
      },
      verification: {
        isGenuine: r.verification?.isGenuine ?? null,
        cardStatus: toCardStatus(r.verification?.cardStatus),
        verifiedAtUtc: r.verification?.verifiedAtUtc ?? null,
        vgAvailable: r.verification?.vgAvailable ?? false,
      },
    };
  }
}
