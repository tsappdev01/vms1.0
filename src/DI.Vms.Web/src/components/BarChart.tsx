import { useState } from 'react';

/* Ranked horizontal bar: the job is comparing magnitude, so colour is sequential
   (one hue), not categorical. A single series needs no legend - the card title
   says what is plotted. Bars are capped well under 24px, with a 4px rounded
   data-end and a square baseline edge. A table view is always available. */

export interface BarDatum {
  label: string;
  value: number;
}

export function BarChart({ data, unit = 'visitors' }: { data: BarDatum[]; unit?: string }) {
  const [asTable, setAsTable] = useState(false);
  const max = Math.max(1, ...data.map((d) => d.value));
  const total = data.reduce((s, d) => s + d.value, 0);

  if (data.length === 0) {
    return <div className="empty"><strong>No data for today</strong></div>;
  }

  if (asTable) {
    return (
      <>
        <div className="table-wrap">
          <table className="data">
            <thead>
              <tr><th>Entity</th><th style={{ textAlign: 'right' }}>{unit}</th><th style={{ textAlign: 'right' }}>Share</th></tr>
            </thead>
            <tbody>
              {data.map((d) => (
                <tr key={d.label}>
                  <td>{d.label}</td>
                  <td className="num">{d.value}</td>
                  <td className="num">{total ? Math.round((d.value / total) * 100) : 0}%</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <ViewToggle asTable={asTable} onToggle={() => setAsTable(false)} />
      </>
    );
  }

  return (
    <>
      <div className="bar-row" role="img" aria-label={`${unit} by DI entity`}>
        {data.map((d) => (
          <div className="bar-track" key={d.label} title={`${d.label}: ${d.value} ${unit}`}>
            <div className="cat">{d.label}</div>
            <div className="bar-shell">
              <div className="bar-fill" style={{ width: `${(d.value / max) * 100}%` }} />
            </div>
            <div className="val">{d.value}</div>
          </div>
        ))}
      </div>
      <ViewToggle asTable={asTable} onToggle={() => setAsTable(true)} />
    </>
  );
}

function ViewToggle({ asTable, onToggle }: { asTable: boolean; onToggle: () => void }) {
  return (
    <div style={{ marginTop: 12, textAlign: 'right' }} className="no-print">
      <button onClick={onToggle} style={{ fontSize: 12 }}>
        {asTable ? 'Show chart' : 'Show as table'}
      </button>
    </div>
  );
}
