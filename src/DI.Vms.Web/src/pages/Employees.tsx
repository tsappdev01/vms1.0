import { useState } from 'react';
import { PageHeader } from '../components/PageHeader';
import { EmptyState } from '../components/EmptyState';
import { useEmployees } from '../api/hooks';

export function Employees() {
  const { data: employees, isLoading } = useEmployees();
  const [term, setTerm] = useState('');

  const filtered = employees?.filter((e) =>
    `${e.name} ${e.employeeCode} ${e.department ?? ''} ${e.entityName}`
      .toLowerCase().includes(term.toLowerCase()));

  return (
    <>
      <PageHeader
        title="Employees / Hosts"
        description="The host master. Reception searches this instead of typing host details, and department, floor and office auto-populate from the match."
        actions={<button className="primary" disabled title="Requires DI.Vms.Api">Add host</button>}
      />

      <div className="toolbar">
        <input
          className="grow"
          placeholder="Search host name, code or department…"
          value={term}
          onChange={(e) => setTerm(e.target.value)}
          aria-label="Search hosts"
        />
      </div>

      <section className="card">
        {isLoading ? <div className="empty">Loading…</div>
          : filtered && filtered.length > 0 ? (
            <div className="table-wrap">
              <table className="data">
                <thead>
                  <tr>
                    <th>Code</th><th>Name</th><th>Entity</th><th>Department</th>
                    <th>Designation</th><th>Floor</th><th>Office</th><th>Email</th><th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((e) => (
                    <tr key={e.id}>
                      <td className="mono">{e.employeeCode}</td>
                      <td style={{ fontWeight: 550 }}>{e.name}</td>
                      <td>{e.entityName}</td>
                      <td>{e.department ?? '—'}</td>
                      <td>{e.designation ?? '—'}</td>
                      <td className="mono">{e.floor ?? '—'}</td>
                      <td className="mono">{e.office ?? '—'}</td>
                      <td>{e.email ?? '—'}</td>
                      <td>{e.isActive ? 'Active' : 'Inactive'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <EmptyState title="No hosts match" />}
      </section>
    </>
  );
}
