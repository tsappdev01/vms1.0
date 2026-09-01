/* A headline number is a stat tile, not a one-bar chart. No hover layer: there is
   no plot to interrogate. */

export function StatTile({
  label, value, note, accent,
}: {
  label: string;
  value: number | string;
  note?: string;
  accent?: string;
}) {
  return (
    <div className="stat-tile">
      <div className="label">
        {accent && <span className="dot" style={{ background: accent, width: 8, height: 8, borderRadius: '50%' }} aria-hidden="true" />}
        {label}
      </div>
      <div className="value">{value}</div>
      {note && <div className="note">{note}</div>}
    </div>
  );
}
