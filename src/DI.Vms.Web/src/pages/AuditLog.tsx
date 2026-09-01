import { PageHeader } from '../components/PageHeader';
import { useAudit } from '../api/hooks';
import { formatDateTime } from '../lib/format';

export function AuditLog() {
  const { data: entries, isLoading, refetch } = useAudit();

  return (
    <>
      <PageHeader
        title="Audit Log"
        description="Append-only record of every modification. Viewing an unmasked ID number is itself an audited event."
        actions={<button onClick={() => refetch()}>Refresh</button>}
      />
      <section className="card">
        {isLoading ? <div className="empty">Loading…</div> : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>When</th><th>User</th><th>Action</th><th>Entity</th>
                  <th>Record</th><th>Old</th><th>New</th><th>IP</th><th>Device</th>
                </tr>
              </thead>
              <tbody>
                {entries?.map((a) => (
                  <tr key={a.id}>
                    <td className="mono">{formatDateTime(a.timestampUtc)}</td>
                    <td className="mono">{a.userName}</td>
                    <td><span className="chip">{a.action}</span></td>
                    <td>{a.entityName}</td>
                    <td className="mono">{a.recordRef ?? '—'}</td>
                    <td>{a.oldValue ?? '—'}</td>
                    <td>{a.newValue ?? '—'}</td>
                    <td className="mono">{a.ipAddress ?? '—'}</td>
                    <td className="mono">{a.deviceId ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </>
  );
}
