/* The API speaks UTC; the portal displays Gulf Standard Time (UTC+4).
   GST has no daylight saving, so a fixed IANA zone is safe. */

const GST = 'Asia/Dubai';

export function formatTime(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleTimeString('en-GB', {
    hour: '2-digit', minute: '2-digit', timeZone: GST,
  });
}

export function formatDateTime(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit', timeZone: GST,
  });
}

export function formatDate(value: string | null): string {
  if (!value) return '—';
  return new Date(value).toLocaleDateString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric', timeZone: GST,
  });
}

/** Elapsed time as "1h 36m", per the BRD's duration example. */
export function formatDuration(fromIso: string | null, toIso: string | null): string {
  if (!fromIso) return '—';
  const end = toIso ? new Date(toIso).getTime() : Date.now();
  const mins = Math.max(0, Math.round((end - new Date(fromIso).getTime()) / 60000));
  const h = Math.floor(mins / 60);
  const m = mins % 60;
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

export function titleCase(value: string): string {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2');
}
