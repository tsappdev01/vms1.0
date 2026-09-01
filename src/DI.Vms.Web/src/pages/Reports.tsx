import { PageHeader } from '../components/PageHeader';

/* The BRD 20 report set. Each becomes a query against DI.Vms.Api with
   json/csv/xlsx output; the list is here so the scope is visible in the UI. */

const REPORTS = [
  { name: 'Daily Visitor Report', detail: 'Visitor, company, ID type, ID no., entity, host, in, out, duration.' },
  { name: 'Visitor by Entity', detail: 'Visit counts grouped by DI entity.' },
  { name: 'Visitor by Host', detail: 'Visit counts grouped by host employee.' },
  { name: 'Visitor by Company', detail: 'Visit counts grouped by visiting company.' },
  { name: 'Visitor by Date', detail: 'Visit counts over a date range.' },
  { name: 'Currently Inside', detail: 'Point-in-time occupancy snapshot.' },
  { name: 'Visitors Never Checked Out', detail: 'Visits auto-closed by the nightly job.' },
  { name: 'Frequent Visitors', detail: 'Visitors ranked by visit count.' },
  { name: 'Expired ID Report', detail: 'Visitors whose presented ID has expired.' },
  { name: 'Visitor Activity by Floor', detail: 'Visits grouped by floor.' },
  { name: 'Security User Activity', detail: 'Actions per security user, from the audit log.' },
];

export function Reports() {
  return (
    <>
      <PageHeader
        title="Reports"
        description="Standard reports. Exports obey the same ID masking as the screen."
      />

      <div className="banner no-print">
        <strong>Not yet available.</strong>
        <span>Reports require DI.Vms.Api. This is the agreed scope.</span>
      </div>

      <section className="card">
        <div className="table-wrap">
          <table className="data">
            <thead><tr><th>Report</th><th>Contents</th><th /></tr></thead>
            <tbody>
              {REPORTS.map((r) => (
                <tr key={r.name}>
                  <td style={{ fontWeight: 550 }}>{r.name}</td>
                  <td style={{ color: 'var(--text-secondary)' }}>{r.detail}</td>
                  <td style={{ textAlign: 'right' }}>
                    <button disabled title="Requires DI.Vms.Api">Run</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </>
  );
}
