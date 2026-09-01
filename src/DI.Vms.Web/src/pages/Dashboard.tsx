import { Link } from 'react-router-dom';
import { BarChart } from '../components/BarChart';
import { EmptyState } from '../components/EmptyState';
import { PageHeader } from '../components/PageHeader';
import { StatTile } from '../components/StatTile';
import { StatusBadge } from '../components/StatusBadge';
import { useDashboardSummary, useInside } from '../api/hooks';
import { formatTime } from '../lib/format';

export function Dashboard() {
  const summary = useDashboardSummary();
  const inside = useInside();

  return (
    <>
      <PageHeader
        title="Dashboard"
        description="Live visitor activity for the DIP office."
      />

      {/* Four headline numbers: a KPI row of stat tiles, not a chart. */}
      <div className="kpi-row">
        <StatTile label="Total Visitors" value={summary.data?.totalToday ?? '—'} note="Today" />
        <StatTile label="Currently Inside" value={summary.data?.currentlyInside ?? '—'} note="On the premises now" accent="var(--status-good)" />
        <StatTile label="Checked Out" value={summary.data?.checkedOut ?? '—'} note="Today" />
        <StatTile label="Expected" value={summary.data?.expected ?? '—'} note="Pre-registered" accent="var(--status-warning)" />
      </div>

      <div className="grid-2">
        <section className="card">
          <div className="card-head">
            <h2>Currently Inside</h2>
            <Link to="/current" className="sub">View all</Link>
          </div>
          {inside.isLoading ? (
            <div className="empty">Loading…</div>
          ) : inside.data && inside.data.length > 0 ? (
            <div className="table-wrap">
              <table className="data">
                <thead>
                  <tr>
                    <th>Visitor</th><th>Visiting</th>
                    <th>Floor</th><th>In Time</th><th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {inside.data.slice(0, 8).map((v) => (
                    <tr key={v.id}>
                      <td>
                        <div style={{ fontWeight: 550 }}>{v.visitorName}</div>
                        {v.company && <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{v.company}</div>}
                      </td>
                      <td>{v.hostName}</td>
                      <td className="mono">{v.floor ?? '—'}</td>
                      <td className="mono">{formatTime(v.inTimeUtc)}</td>
                      <td><StatusBadge status={v.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <EmptyState title="Nobody is currently inside" />
          )}
        </section>

        <section className="card">
          <div className="card-head">
            <h2>Visitors by DI Entity</h2>
            <span className="sub">Today</span>
          </div>
          <div className="card-body">
            <BarChart data={(summary.data?.visitorsByEntity ?? []).map((e) => ({ label: e.entityName, value: e.visitors }))} />
          </div>
        </section>
      </div>
    </>
  );
}
