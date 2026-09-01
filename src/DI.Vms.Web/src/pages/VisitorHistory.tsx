import { useState } from 'react';
import { PageHeader } from '../components/PageHeader';
import { StatusBadge } from '../components/StatusBadge';
import { MaskedId } from '../components/MaskedId';
import { EmptyState } from '../components/EmptyState';
import { useCurrentUser, useEmployees, useVisitSearch } from '../api/hooks';
import { formatDate, formatDuration, formatTime, titleCase } from '../lib/format';
import type { VisitStatus } from '../api/types';

export function VisitorHistory() {
  const [term, setTerm] = useState('');
  const [status, setStatus] = useState<VisitStatus | ''>('');
  const [host, setHost] = useState('');

  const { data: hosts } = useEmployees();
  const { data: result, isLoading } = useVisitSearch({ q: term, status, host });
  const { data: user } = useCurrentUser();
  const canUnmask = user?.canViewUnmaskedId ?? false;

  return (
    <>
      <PageHeader
        title="Visitor History"
        description="Search by visitor name, company, visit number or ID number."
      />

      <div className="toolbar">
        <input
          className="grow"
          placeholder="Search name, company, visit number…"
          value={term}
          onChange={(e) => setTerm(e.target.value)}
          aria-label="Search visitors"
        />
        <select value={status} onChange={(e) => setStatus(e.target.value as VisitStatus | '')} aria-label="Status">
          <option value="">Any status</option>
          <option value="Inside">Inside</option>
          <option value="CheckedOut">Checked Out</option>
          <option value="Expected">Expected</option>
        </select>
        <select value={host} onChange={(e) => setHost(e.target.value)} aria-label="Host">
          <option value="">Any host</option>
          {hosts?.map((h) => <option key={h.id} value={h.name}>{h.name}</option>)}
        </select>
        {(term || status || host) && (
          <button onClick={() => { setTerm(''); setStatus(''); setHost(''); }}>Clear</button>
        )}
      </div>

      <section className="card">
        <div className="card-head">
          <h2>Results</h2>
          <span className="sub">{result ? `${result.totalCount} ${result.totalCount === 1 ? 'visit' : 'visits'}` : ''}</span>
        </div>
        {isLoading ? (
          <div className="empty">Searching…</div>
        ) : result && result.items.length > 0 ? (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Visit No.</th><th>Date</th><th>Visitor</th><th>Company</th>
                  <th>ID Number</th><th>Type</th><th>Host</th><th>Entity</th>
                  <th>In</th><th>Out</th><th>Duration</th><th>Status</th>
                </tr>
              </thead>
              <tbody>
                {result.items.map((v) => (
                  <tr key={v.id}>
                    <td className="mono">{v.visitNumber || '—'}</td>
                    <td>{formatDate(v.inTimeUtc ?? v.expectedDate)}</td>
                    <td>{v.visitorName}</td>
                    <td>{v.company ?? '—'}</td>
                    <td><MaskedId masked={v.idNumberMasked} visitId={v.id} canUnmask={canUnmask} /></td>
                    <td>{titleCase(v.visitType)}</td>
                    <td>{v.hostName}</td>
                    <td>{v.entityName}</td>
                    <td className="mono">{formatTime(v.inTimeUtc)}</td>
                    <td className="mono">{formatTime(v.outTimeUtc)}</td>
                    <td className="mono">{v.outTimeUtc ? formatDuration(v.inTimeUtc, v.outTimeUtc) : '—'}</td>
                    <td><StatusBadge status={v.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState title="No visits match" hint="Try a shorter search term or clear the filters." />
        )}
      </section>
    </>
  );
}
