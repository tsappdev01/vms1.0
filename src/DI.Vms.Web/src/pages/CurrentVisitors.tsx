import { PageHeader } from '../components/PageHeader';
import { StatusBadge, VerificationBadge } from '../components/StatusBadge';
import { MaskedId } from '../components/MaskedId';
import { EmptyState } from '../components/EmptyState';
import { useCurrentUser, useInside } from '../api/hooks';
import { formatDuration, formatTime, titleCase } from '../lib/format';

export function CurrentVisitors() {
  const { data: visits, isLoading } = useInside();
  const { data: user } = useCurrentUser();
  const canUnmask = user?.canViewUnmaskedId ?? false;

  return (
    <>
      <PageHeader
        title="Current Visitors"
        description="Everyone checked in and not yet checked out."
        actions={<button onClick={() => window.print()}>Print</button>}
      />

      <section className="card">
        {isLoading ? (
          <div className="empty">Loading…</div>
        ) : visits && visits.length > 0 ? (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Visit No.</th><th>Visitor</th><th>ID Number</th>
                  <th>Type</th><th>Visiting</th><th>Floor</th>
                  <th>In Time</th><th>Duration</th><th>Verification</th><th>Status</th>
                </tr>
              </thead>
              <tbody>
                {visits.map((v) => (
                  <tr key={v.id}>
                    <td className="mono">{v.visitNumber}</td>
                    <td>
                      <div style={{ fontWeight: 550 }}>{v.visitorName}</div>
                      {v.company && <div className="sub-cell">{v.company}</div>}
                    </td>
                    <td><MaskedId masked={v.idNumberMasked} visitId={v.id} canUnmask={canUnmask} /></td>
                    <td>{titleCase(v.visitType)}</td>
                    <td>
                      <div>{v.hostName}</div>
                      <div className="sub-cell">{v.entityName}</div>
                    </td>
                    <td className="mono">{v.floor ?? '—'}</td>
                    <td className="mono">{formatTime(v.inTimeUtc)}</td>
                    <td className="mono">{formatDuration(v.inTimeUtc, null)}</td>
                    <td><VerificationBadge state={v.verification} /></td>
                    <td><StatusBadge status={v.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState title="Nobody is currently inside" hint="Visitors appear here as soon as reception checks them in." />
        )}
      </section>
    </>
  );
}
