import { useState } from 'react';
import { api } from '../api';

/* ID numbers render masked by default (BRD 22). Unmasking is a deliberate,
   permission-gated action that writes a VIEW-UNMASKED-ID audit row server-side -
   looking at an ID number is itself an auditable event. */

export function MaskedId({
  masked, visitId, canUnmask,
}: {
  masked: string;
  visitId: string;
  canUnmask: boolean;
}) {
  const [revealed, setRevealed] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function reveal() {
    setBusy(true);
    setError(null);
    try {
      setRevealed(await api.getUnmaskedId(visitId));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to retrieve the ID number.');
    } finally {
      setBusy(false);
    }
  }

  if (revealed) {
    return (
      <span className="masked">
        <span>{revealed}</span>
        <button onClick={() => setRevealed(null)} title="Hide the ID number again">Hide</button>
      </span>
    );
  }

  return (
    <span className="masked">
      <span>{masked}</span>
      {canUnmask && masked.includes('X') && (
        <button onClick={reveal} disabled={busy} title="Reveal the full ID number. This is audited.">
          {busy ? '…' : 'Unmask'}
        </button>
      )}
      {error && <span style={{ color: 'var(--status-critical)', fontSize: 11 }}>{error}</span>}
    </span>
  );
}
