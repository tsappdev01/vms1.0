import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { PageHeader } from '../components/PageHeader';
import { EmptyState } from '../components/EmptyState';
import { api } from '../api';
import type { ReportDefinition, ReportResult } from '../api/types';
import { formatDateTime } from '../lib/format';

/* The BRD 20 report set. Every report is a table, so one screen renders all of them
   rather than eleven bespoke pages.

   ID numbers appear masked, here and in exports: an unmasked spreadsheet leaving the
   building would defeat the control in BRD 22. */

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

export function Reports() {
  const [selected, setSelected] = useState<ReportDefinition | null>(null);
  const [from, setFrom] = useState(today());
  const [to, setTo] = useState(today());
  const [result, setResult] = useState<ReportResult | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: definitions, isLoading } = useQuery({
    queryKey: ['reports'],
    queryFn: () => api.getReports(),
  });

  async function run(def: ReportDefinition) {
    setSelected(def);
    setBusy(true);
    setError(null);
    setResult(null);
    try {
      setResult(def.takesDateRange
        ? await api.runReport(def.name, from, to)
        : await api.runReport(def.name));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The report could not be run.');
    } finally {
      setBusy(false);
    }
  }

  function downloadCsv() {
    if (!result) return;

    const escape = (v: string | null) => {
      if (!v) return '';
      // Neutralise anything a spreadsheet would treat as a formula.
      const s = /^[=+\-@\t\r]/.test(v) ? `'${v}` : v;
      return /[",\n\r]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
    };

    const lines = [
      result.title,
      result.from && result.to ? `${result.from} to ${result.to}` : '',
      `Generated ${formatDateTime(result.generatedAtUtc)} GST`,
      '',
      result.columns.map(escape).join(','),
      ...result.rows.map((r) => r.map(escape).join(',')),
    ];

    const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${result.name}-${result.from ?? today()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <>
      <PageHeader
        title="Reports"
        description="Exports carry the same ID masking as the screen."
      />

      {error && (
        <div className="banner" style={{ borderColor: 'var(--status-critical)' }}>
          <strong>Problem.</strong><span>{error}</span>
        </div>
      )}

      <div className="toolbar">
        <label style={{ fontSize: 12.5, color: 'var(--text-secondary)' }}>
          From{' '}
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>
        <label style={{ fontSize: 12.5, color: 'var(--text-secondary)' }}>
          To{' '}
          <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </label>
        {selected?.takesDateRange && (
          <button onClick={() => run(selected)} disabled={busy}>Re-run with this range</button>
        )}
      </div>

      <div className="grid-2">
        <section className="card">
          <div className="card-head"><h2>Available reports</h2></div>
          {isLoading ? <div className="empty">Loading…</div> : (
            <div className="table-wrap">
              <table className="data">
                <tbody>
                  {definitions?.map((d) => (
                    <tr key={d.name} style={{ background: selected?.name === d.name ? 'var(--surface-2)' : undefined }}>
                      <td>
                        <div style={{ fontWeight: 550 }}>{d.title}</div>
                        <div className="sub-cell">{d.description}</div>
                        {!d.takesDateRange && <div className="sub-cell">Not date-ranged.</div>}
                      </td>
                      <td style={{ textAlign: 'right', width: 84 }}>
                        <button onClick={() => run(d)} disabled={busy}>
                          {busy && selected?.name === d.name ? '…' : 'Run'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="card">
          <div className="card-head">
            <h2>{result ? result.title : 'Result'}</h2>
            {result && (
              <div style={{ display: 'flex', gap: 8, alignItems: 'baseline' }}>
                <span className="sub">{result.rows.length} {result.rows.length === 1 ? 'row' : 'rows'}</span>
                <button onClick={downloadCsv} style={{ fontSize: 12 }} disabled={result.rows.length === 0}>
                  Export CSV
                </button>
              </div>
            )}
          </div>

          {busy ? <div className="empty">Running…</div>
            : !result ? <EmptyState title="Choose a report" hint="Pick one from the list and press Run." />
            : result.rows.length === 0 ? (
              <EmptyState
                title="No rows"
                hint={result.from ? `Nothing matched between ${result.from} and ${result.to}.` : 'Nothing to report.'}
              />
            ) : (
              <div className="table-wrap">
                <table className="data">
                  <thead>
                    <tr>{result.columns.map((c) => <th key={c}>{c}</th>)}</tr>
                  </thead>
                  <tbody>
                    {result.rows.map((row, i) => (
                      // Row order is the report's own ordering; index is the only stable key.
                      <tr key={i}>
                        {row.map((cell, j) => (
                          <td key={j} className={j > 0 && /^\d+$/.test(cell ?? '') ? 'num' : undefined}>
                            {cell ?? '—'}
                          </td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          {result && (
            <div style={{ padding: '10px 16px', borderTop: '1px solid var(--border)', fontSize: 11.5, color: 'var(--text-muted)' }}>
              Generated {formatDateTime(result.generatedAtUtc)} GST
            </div>
          )}
        </section>
      </div>
    </>
  );
}
