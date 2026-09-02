/* Real API client. Shape-compatible with mockClient, so the portal is written
   against one interface and the source is a configuration choice. */

import type { ApiClient } from './mock/mockClient';
import type {
  CheckInResponse, CheckOutResponse, CreateVisitRequest, CreateVisitResponse,
  IdentifyRequest, IdentifyResponse, VisitorSearchQuery,
} from './types';

const BASE = (import.meta.env.VITE_API_URL ?? '') + '/api/v1';

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { Accept: 'application/json' },
    credentials: 'include',
  });

  if (!res.ok) {
    /* The API returns RFC 7807 problem+json; surface its detail when present. */
    let detail = `${res.status} ${res.statusText}`;
    try {
      const problem = await res.json();
      if (problem?.detail) detail = problem.detail;
      else if (problem?.title) detail = problem.title;
    } catch {
      /* Non-JSON error body; keep the status line. */
    }
    throw new Error(detail);
  }

  return res.json() as Promise<T>;
}

async function post<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    credentials: 'include',
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    let detail = `${res.status} ${res.statusText}`;
    try {
      const problem = await res.json();
      /* RFC 7807, plus the per-field errors ValidationProblem returns. */
      if (problem?.errors) {
        detail = Object.values(problem.errors as Record<string, string[]>).flat().join(' ');
      } else if (problem?.detail) detail = problem.detail;
      else if (problem?.title) detail = problem.title;
    } catch {
      /* Non-JSON error body; keep the status line. */
    }
    throw new Error(detail);
  }

  return res.json() as Promise<T>;
}

function query(q: VisitorSearchQuery): string {
  const p = new URLSearchParams();
  for (const [k, v] of Object.entries(q)) {
    if (v !== undefined && v !== null && v !== '') p.set(k, String(v));
  }
  const s = p.toString();
  return s ? `?${s}` : '';
}

export const httpClient: ApiClient = {
  getDashboardSummary: () => get('/dashboard/summary'),
  getInside: () => get('/visits/inside'),
  getExpected: () => get('/visits/expected'),
  getOccupancy: () => get('/emergency/occupancy'),
  searchVisits: (q) => get(`/visitors/search${query(q)}`),
  getVisitorProfile: (name) => get(`/visitors/${encodeURIComponent(name)}/history`),
  getUnmaskedId: (visitId) => get(`/visitors/${encodeURIComponent(visitId)}/id-number`),
  getEntities: () => get('/entities'),
  getEmployees: () => get('/employees'),
  getUsers: () => get('/users'),
  getAudit: () => get('/audit'),
  getCurrentUser: () => get('/me'),
  identify: (r: IdentifyRequest) => post<IdentifyResponse>('/visits/identify', r),
  createVisit: (r: CreateVisitRequest) => post<CreateVisitResponse>('/visits', r),
  checkIn: (id: string, signatureImage: string, deviceId: string) =>
    post<CheckInResponse>(`/visits/${id}/check-in`, { signatureImage, deviceId }),
  checkOut: (id: string) => post<CheckOutResponse>(`/visits/${id}/check-out`, {}),
};
