import { PageHeader } from '../components/PageHeader';
import { StatusBadge } from '../components/StatusBadge';
import { EmptyState } from '../components/EmptyState';
import { useExpected } from '../api/hooks';
import { formatDate, titleCase } from '../lib/format';

export function ExpectedVisitors() {
  const { data: visits, isLoading } = useExpected();

  return (
    <>
      <PageHeader
        title="Expected Visitors"
        description="Pre-registered visitors who have not yet arrived. Check-in happens at reception."
      />

      <section className="card">
        {isLoading ? (
          <div className="empty">Loading…</div>
        ) : visits && visits.length > 0 ? (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Visitor</th><th>Company</th><th>Host</th><th>Entity</th>
                  <th>Expected Date</th><th>Expected Time</th><th>Type</th><th>Purpose</th><th>Status</th>
                </tr>
              </thead>
              <tbody>
                {visits.map((v) => (
                  <tr key={v.id}>
                    <td>{v.visitorName}</td>
                    <td>{v.company ?? '—'}</td>
                    <td>{v.hostName}</td>
                    <td>{v.entityName}</td>
                    <td>{formatDate(v.expectedDate)}</td>
                    <td className="mono">{v.expectedTime ?? '—'}</td>
                    <td>{titleCase(v.visitType)}</td>
                    <td>{v.purpose ?? '—'}</td>
                    <td><StatusBadge status={v.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState title="No expected visitors" hint="Pre-registration arrives in Phase 2." />
        )}
      </section>
    </>
  );
}
