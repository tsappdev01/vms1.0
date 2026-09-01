import type { VerificationState, VisitStatus } from '../api/types';

/* Status is conveyed by a coloured dot AND a text label - never by colour alone.
   Colours come from the fixed status palette and are not reused as series colours. */

const VISIT_STATUS: Record<VisitStatus, { colour: string; label: string }> = {
  Inside:     { colour: 'var(--status-good)',   label: 'Inside' },
  CheckedOut: { colour: 'var(--text-muted)',    label: 'Checked Out' },
  Expected:   { colour: 'var(--status-warning)', label: 'Expected' },
  Cancelled:  { colour: 'var(--baseline)',      label: 'Cancelled' },
};

const VERIFICATION: Record<VerificationState, { colour: string; label: string }> = {
  Verified:  { colour: 'var(--status-good)',     label: 'Verified' },
  IdExpired: { colour: 'var(--status-warning)',  label: 'ID Expired' },
  Pending:   { colour: 'var(--status-serious)',  label: 'Pending' },
  Failed:    { colour: 'var(--status-critical)', label: 'Failed' },
};

export function StatusBadge({ status }: { status: VisitStatus }) {
  const s = VISIT_STATUS[status];
  return (
    <span className="status">
      <span className="dot" style={{ background: s.colour }} aria-hidden="true" />
      {s.label}
    </span>
  );
}

export function VerificationBadge({ state }: { state: VerificationState }) {
  const s = VERIFICATION[state];
  return (
    <span className="status" title={`Card verification: ${s.label}`}>
      <span className="dot" style={{ background: s.colour }} aria-hidden="true" />
      {s.label}
    </span>
  );
}
