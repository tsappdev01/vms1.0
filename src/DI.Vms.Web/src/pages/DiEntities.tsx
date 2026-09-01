import { PageHeader } from '../components/PageHeader';
import { useEntities } from '../api/hooks';

export function DiEntities() {
  const { data: entities, isLoading } = useEntities();

  return (
    <>
      <PageHeader
        title="DI Entities"
        description="The centralised list of companies within the DI group."
        actions={<button className="primary" disabled title="Requires DI.Vms.Api">Add entity</button>}
      />
      <section className="card">
        {isLoading ? <div className="empty">Loading…</div> : (
          <div className="table-wrap">
            <table className="data">
              <thead><tr><th>Code</th><th>Entity Name</th><th>Status</th></tr></thead>
              <tbody>
                {entities?.map((e) => (
                  <tr key={e.id}>
                    <td className="mono">{e.entityCode}</td>
                    <td>{e.entityName}</td>
                    <td>{e.isActive ? 'Active' : 'Inactive'}</td>
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
