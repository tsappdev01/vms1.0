import { PageHeader } from '../components/PageHeader';
import { useOccupancy } from '../api/hooks';
import { formatTime, titleCase } from '../lib/format';
import type { OccupancyPerson } from '../api/types';

/* BRD 12 treats this as mandatory, not a report. Design consequences:
   - one click from every screen (it is a top-level nav item)
   - grouped by floor, because that is how a building is swept
   - unpaged: during an evacuation nobody should be clicking "next page"
   - printable and downloadable, because the assembly point has no wifi
   - available to every authenticated role */

export function Emergency() {
  const { data: people, isLoading, dataUpdatedAt } = useOccupancy();

  const byFloor = new Map<string, OccupancyPerson[]>();
  for (const p of people ?? []) {
    const key = p.floor ?? 'Unknown';
    const list = byFloor.get(key) ?? [];
    list.push(p);
    byFloor.set(key, list);
  }
  const floors = [...byFloor.entries()].sort((a, b) => {
    const na = Number(a[0]); const nb = Number(b[0]);
    if (Number.isNaN(na) || Number.isNaN(nb)) return a[0].localeCompare(b[0]);
    return na - nb;
  });

  function download() {
    const lines = [
      'PEOPLE CURRENTLY INSIDE - DI DIP OFFICE',
      `Generated ${new Date().toLocaleString('en-GB', { timeZone: 'Asia/Dubai' })} (GST)`,
      `Total on site: ${people?.length ?? 0}`,
      '',
    ];
    for (const [floor, list] of floors) {
      lines.push(`FLOOR ${floor} (${list.length})`);
      lines.push('Name'.padEnd(26) + 'Host'.padEnd(22) + 'Category'.padEnd(16) + 'In Time');
      for (const p of list) {
        lines.push(
          p.name.padEnd(26) +
          (p.hostName ?? '—').padEnd(22) +
          titleCase(p.category).padEnd(16) +
          formatTime(p.inTimeUtc),
        );
      }
      lines.push('');
    }

    const blob = new Blob([lines.join('\n')], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `evacuation-list-${new Date().toISOString().slice(0, 16).replace(/[:T]/g, '')}.txt`;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <>
      <PageHeader
        title="People Currently Inside"
        description="Evacuation and accountability list. Grouped by floor."
        actions={
          <>
            <button onClick={() => window.print()}>Print</button>
            <button className="primary" onClick={download}>Download</button>
          </>
        }
      />

      <div className="kpi-row">
        <div className="stat-tile">
          <div className="label">Total On Site</div>
          <div className="value">{people?.length ?? '—'}</div>
          <div className="note">
            {dataUpdatedAt ? `Updated ${formatTime(new Date(dataUpdatedAt).toISOString())}` : 'Loading…'}
          </div>
        </div>
        <div className="stat-tile">
          <div className="label">Floors Occupied</div>
          <div className="value">{floors.length || '—'}</div>
          <div className="note">Sweep in this order</div>
        </div>
      </div>

      <div className="banner no-print">
        <strong>Employees are not yet included.</strong>
        <span>
          This list covers visitors, contractors and service providers. Employee
          presence requires the directory integration scheduled for Phase 3.
        </span>
      </div>

      <section className="card">
        <div className="card-body">
          {isLoading ? (
            <div className="empty">Loading…</div>
          ) : floors.length === 0 ? (
            <div className="empty"><strong>The building is empty</strong><span>Nobody is currently checked in.</span></div>
          ) : (
            /* One table with a group row per floor, rather than a table per floor:
               the columns then line up down the whole page, which is what makes a
               sweep list scannable. */
            <div className="table-wrap">
              <table className="data occupancy">
                <colgroup>
                  <col style={{ width: '22%' }} /><col style={{ width: '20%' }} />
                  <col style={{ width: '18%' }} /><col style={{ width: '14%' }} />
                  <col style={{ width: '10%' }} /><col style={{ width: '16%' }} />
                </colgroup>
                <thead>
                  <tr><th>Name</th><th>Company</th><th>Host</th><th>Category</th><th>In Time</th><th>Visit No.</th></tr>
                </thead>
                {floors.map(([floor, list]) => (
                  <tbody key={floor}>
                    <tr className="group-row">
                      <th colSpan={6} scope="colgroup">
                        Floor {floor}
                        <span className="count"> · {list.length} {list.length === 1 ? 'person' : 'people'}</span>
                      </th>
                    </tr>
                    {list.map((p) => (
                      <tr key={`${p.name}-${p.visitNumber}`}>
                        <td style={{ fontWeight: 550 }}>{p.name}</td>
                        <td>{p.company ?? '—'}</td>
                        <td>{p.hostName ?? '—'}</td>
                        <td>{titleCase(p.category)}</td>
                        <td className="mono">{formatTime(p.inTimeUtc)}</td>
                        <td className="mono">{p.visitNumber ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                ))}
              </table>
            </div>
          )}
        </div>
      </section>
    </>
  );
}
