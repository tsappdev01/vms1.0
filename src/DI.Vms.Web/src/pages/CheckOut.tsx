import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { PageHeader } from '../components/PageHeader';
import { EmptyState } from '../components/EmptyState';
import { api } from '../api';
import { useInside } from '../api/hooks';
import { formatDuration, formatTime } from '../lib/format';

export function CheckOut() {
  const { data: visits, isLoading } = useInside();
  const queryClient = useQueryClient();
  const [term, setTerm] = useState('');
  const [busyId, setBusyId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState<{ visitNumber: string; duration: number } | null>(null);

  const filtered = visits?.filter((v) =>
    `${v.visitorName} ${v.company ?? ''} ${v.visitNumber}`.toLowerCase().includes(term.toLowerCase()));

  async function checkOut(id: string, visitNumber: string) {
    setBusyId(id);
    setError(null);
    try {
      const result = await api.checkOut(id);
      setDone({ visitNumber, duration: result.durationMinutes });
      /* Every occupancy view is now stale - the dashboard, the current-visitors
         table and the evacuation list all derive from this. */
      await queryClient.invalidateQueries();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Check-out failed.');
    } finally {
      setBusyId(null);
    }
  }

  return (
    <>
      <PageHeader title="Check Out" description="Search for the visitor, then check them out." />

      {done && (
        <div className="banner">
          <strong>{done.visitNumber} checked out.</strong>
          <span>Duration {Math.floor(done.duration / 60)}h {done.duration % 60}m.</span>
        </div>
      )}
      {error && (
        <div className="banner" style={{ borderColor: 'var(--status-critical)' }}>
          <strong>Problem.</strong><span>{error}</span>
        </div>
      )}

      <div className="toolbar">
        <input
          className="grow"
          placeholder="Search visitor name, company or visit number…"
          value={term}
          onChange={(e) => setTerm(e.target.value)}
          aria-label="Search visitors inside"
        />
      </div>

      <section className="card">
        {isLoading ? <div className="empty">Loading…</div>
          : filtered && filtered.length > 0 ? (
            <div className="table-wrap">
              <table className="data">
                <thead>
                  <tr>
                    <th>Visit No.</th><th>Visitor</th><th>Visiting</th>
                    <th>Floor</th><th>In Time</th><th>Duration</th><th />
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((v) => (
                    <tr key={v.id}>
                      <td className="mono">{v.visitNumber}</td>
                      <td>
                        <div style={{ fontWeight: 550 }}>{v.visitorName}</div>
                        {v.company && <div className="sub-cell">{v.company}</div>}
                      </td>
                      <td>{v.hostName}</td>
                      <td className="mono">{v.floor ?? '—'}</td>
                      <td className="mono">{formatTime(v.inTimeUtc)}</td>
                      <td className="mono">{formatDuration(v.inTimeUtc, null)}</td>
                      <td style={{ textAlign: 'right' }}>
                        <button
                          className="primary"
                          disabled={busyId === v.id}
                          onClick={() => checkOut(v.id, v.visitNumber)}
                        >
                          {busyId === v.id ? '…' : 'Check out'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <EmptyState
              title={term ? 'No match' : 'Nobody is currently inside'}
              hint={term ? 'Try a shorter search term.' : undefined}
            />
          )}
      </section>
    </>
  );
}
