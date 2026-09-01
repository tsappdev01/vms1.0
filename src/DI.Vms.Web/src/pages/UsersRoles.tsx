import { PageHeader } from '../components/PageHeader';
import { useUsers } from '../api/hooks';
import { titleCase } from '../lib/format';

export function UsersRoles() {
  const { data: users, isLoading } = useUsers();

  return (
    <>
      <PageHeader
        title="Users & Roles"
        description="Unmasked ID access is a separate per-user grant, not implied by any role."
        actions={<button className="primary" disabled title="Requires DI.Vms.Api">Add user</button>}
      />
      <section className="card">
        {isLoading ? <div className="empty">Loading…</div> : (
          <div className="table-wrap">
            <table className="data">
              <thead>
                <tr>
                  <th>Username</th><th>Name</th><th>Role</th>
                  <th>Reception Point</th><th>Unmasked ID</th><th>Status</th>
                </tr>
              </thead>
              <tbody>
                {users?.map((u) => (
                  <tr key={u.id}>
                    <td className="mono">{u.username}</td>
                    <td>{u.name}</td>
                    <td><span className="chip">{titleCase(u.role)}</span></td>
                    <td>{u.securityLocation ?? '—'}</td>
                    <td>
                      {u.canViewUnmaskedId
                        ? <span className="status"><span className="dot" style={{ background: 'var(--status-serious)' }} aria-hidden="true" />Granted</span>
                        : <span style={{ color: 'var(--text-muted)' }}>Not granted</span>}
                    </td>
                    <td>{u.isActive ? 'Active' : 'Inactive'}</td>
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
