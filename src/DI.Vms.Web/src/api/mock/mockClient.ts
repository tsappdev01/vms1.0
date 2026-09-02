/* Mock implementation of the API surface. Selected by VITE_USE_MOCK.
   Deliberately mirrors docs/03-api-specification.md so swapping to the real
   client is a configuration change, not a rewrite. */

import type {
  AuditEntry, CheckInResponse, CheckOutResponse, CreateVisitRequest, CreateVisitResponse,
  DashboardSummary, DiEntityDto, EmployeeDto, IdentifyRequest, IdentifyResponse,
  OccupancyPerson, Paged, UserDto, VisitListItem, VisitorProfile, VisitorSearchQuery,
} from '../types';
import {
  auditEntries, currentUser, entities, employees, occupancy, users, visits,
} from './fixtures';

/** Simulated network latency, so loading states are exercised in development. */
const delay = (ms = 220) => new Promise((r) => setTimeout(r, ms));

function page<T>(items: T[], p = 1, size = 25): Paged<T> {
  const start = (p - 1) * size;
  return { items: items.slice(start, start + size), page: p, pageSize: size, totalCount: items.length };
}

export const mockClient = {
  async getDashboardSummary(): Promise<DashboardSummary> {
    await delay();
    const inside = visits.filter((v) => v.status === 'Inside');
    const out = visits.filter((v) => v.status === 'CheckedOut');
    const expected = visits.filter((v) => v.status === 'Expected');

    const byEntity = new Map<string, number>();
    for (const v of [...inside, ...out]) {
      byEntity.set(v.entityName, (byEntity.get(v.entityName) ?? 0) + 1);
    }

    return {
      totalToday: inside.length + out.length,
      currentlyInside: inside.length,
      checkedOut: out.length,
      expected: expected.length,
      visitorsByEntity: [...byEntity.entries()]
        .map(([entityName, v]) => ({ entityName, visitors: v }))
        .sort((a, b) => b.visitors - a.visitors),
    };
  },

  async getInside(): Promise<VisitListItem[]> {
    await delay();
    return visits
      .filter((v) => v.status === 'Inside')
      .sort((a, b) => (a.inTimeUtc ?? '').localeCompare(b.inTimeUtc ?? ''));
  },

  async getExpected(): Promise<VisitListItem[]> {
    await delay();
    return visits.filter((v) => v.status === 'Expected');
  },

  async getOccupancy(): Promise<OccupancyPerson[]> {
    await delay(120);
    return occupancy;
  },

  async searchVisits(q: VisitorSearchQuery): Promise<Paged<VisitListItem>> {
    await delay();
    const term = q.q?.trim().toLowerCase() ?? '';
    const filtered = visits.filter((v) => {
      if (term && !`${v.visitorName} ${v.company ?? ''} ${v.visitNumber} ${v.idNumberMasked}`.toLowerCase().includes(term)) return false;
      if (q.status && v.status !== q.status) return false;
      if (q.host && v.hostName !== q.host) return false;
      if (q.floor && v.floor !== q.floor) return false;
      return true;
    });
    return page(filtered, q.page ?? 1, q.pageSize ?? 25);
  },

  async getVisitorProfile(name: string): Promise<VisitorProfile | null> {
    await delay();
    const own = visits.filter((v) => v.visitorName === name);
    const first = own[0];
    if (!first) return null;
    return {
      id: first.id,
      name: first.visitorName,
      company: first.company,
      idType: first.idType,
      idNumberMasked: first.idNumberMasked,
      idExpiryDate: '2028-04-15',
      idExpired: first.idExpired,
      nationality: 'PAK',
      totalVisits: 17,
      lastVisitDate: (first.inTimeUtc ?? '').slice(0, 10) || null,
      visits: own,
    };
  },

  /* Permission-gated and audited (BRD 22). The mock records the audit row so the
     control is visible in the UI rather than merely asserted in the docs. */
  async getUnmaskedId(visitId: string): Promise<string> {
    await delay(300);
    if (!currentUser.canViewUnmaskedId) {
      throw new Error('You do not have permission to view unmasked ID numbers.');
    }
    const v = visits.find((x) => x.id === visitId);
    if (!v) throw new Error('Visit not found.');

    auditEntries.unshift({
      id: Math.max(...auditEntries.map((a) => a.id)) + 1,
      userName: currentUser.username,
      action: 'VIEW-UNMASKED-ID',
      entityName: 'Visitor',
      recordRef: v.visitorName,
      oldValue: null,
      newValue: null,
      timestampUtc: new Date().toISOString(),
      ipAddress: '10.20.4.9',
      deviceId: 'PORTAL',
    });

    return v.idType === 'EmiratesId' ? '784-1985-1234567-1' : 'P8821445';
  },

  async getEntities(): Promise<DiEntityDto[]> { await delay(); return entities; },
  async getEmployees(): Promise<EmployeeDto[]> { await delay(); return employees; },
  async getUsers(): Promise<UserDto[]> { await delay(); return users; },
  async getAudit(): Promise<AuditEntry[]> {
    await delay();
    return [...auditEntries].sort((a, b) => b.timestampUtc.localeCompare(a.timestampUtc));
  },
  async getCurrentUser(): Promise<UserDto> { await delay(60); return currentUser; },

  /* ------------------------------------------------------------- write side */

  async identify(request: IdentifyRequest): Promise<IdentifyResponse> {
    await delay(400);
    const digits = request.idNumber.replace(/\D/g, '');
    const existing = visits.find((v) => v.idNumberMasked.endsWith(digits.slice(-1)) && digits.length === 15);

    if (!existing) return { found: false, visitor: null };

    return {
      found: true,
      visitor: {
        id: existing.id,
        name: existing.visitorName,
        company: existing.company,
        idNumberMasked: existing.idNumberMasked,
        idExpiryDate: '2028-04-15',
        idExpired: existing.idExpired,
        totalVisits: visits.filter((v) => v.visitorName === existing.visitorName).length,
        lastVisitDate: (existing.inTimeUtc ?? '').slice(0, 10) || null,
      },
    };
  },

  async createVisit(request: CreateVisitRequest): Promise<CreateVisitResponse> {
    await delay(400);

    const host = employees.find((e) => e.id === request.hostEmployeeId);
    const entity = entities.find((e) => e.id === request.diEntityId);
    if (!host) throw new Error('No employee matches hostEmployeeId.');
    if (!entity) throw new Error('No entity matches diEntityId.');

    const id = `v${Date.now()}`;
    visits.push({
      id,
      visitNumber: '',
      visitorName: request.visitor?.name ?? 'Unknown',
      company: request.visitor?.company ?? null,
      idType: request.visitor?.idType ?? 'EmiratesId',
      idNumberMasked: maskEmiratesId(request.visitor?.idNumber ?? ''),
      idExpired: false,
      hostName: host.name,
      entityName: entity.entityName,
      /* Snapshotted from the host, exactly as the API does it. */
      department: host.department,
      floor: host.floor,
      office: host.office,
      purpose: request.purpose,
      visitType: request.visitType,
      inTimeUtc: null,
      outTimeUtc: null,
      expectedDate: request.expectedDate,
      expectedTime: request.expectedTime,
      status: 'Expected',
      verification: 'Verified',
    });

    return { id, status: 'Expected' };
  },

  async checkIn(id: string, _signatureImage: string, deviceId: string): Promise<CheckInResponse> {
    await delay(400);
    const visit = visits.find((v) => v.id === id);
    if (!visit) throw new Error('Visit not found.');
    if (visit.status === 'Inside') throw new Error(`Visit ${visit.visitNumber} is already checked in.`);

    visit.visitNumber = visit.visitNumber || nextVisitNumber();
    visit.inTimeUtc = new Date().toISOString();
    visit.status = 'Inside';
    occupancy.push({
      name: visit.visitorName,
      company: visit.company,
      hostName: visit.hostName,
      floor: visit.floor,
      category: visit.visitType === 'Contractor' ? 'Contractor' : 'Visitor',
      inTimeUtc: visit.inTimeUtc,
      visitNumber: visit.visitNumber,
    });

    auditEntries.unshift({
      id: Math.max(...auditEntries.map((a) => a.id)) + 1,
      userName: currentUser.username,
      action: 'CHECK-IN',
      entityName: 'Visit',
      recordRef: visit.visitNumber,
      oldValue: null,
      newValue: 'Inside',
      timestampUtc: visit.inTimeUtc,
      ipAddress: '10.20.4.9',
      deviceId,
    });

    return {
      visitNumber: visit.visitNumber,
      inTimeUtc: visit.inTimeUtc,
      status: 'Inside',
      host: { name: visit.hostName, notified: false },
    };
  },

  async checkOut(id: string): Promise<CheckOutResponse> {
    await delay(400);
    const visit = visits.find((v) => v.id === id);
    if (!visit) throw new Error('Visit not found.');
    if (visit.status !== 'Inside') throw new Error(`Visit ${visit.visitNumber} is ${visit.status}.`);

    visit.outTimeUtc = new Date().toISOString();
    visit.status = 'CheckedOut';
    const i = occupancy.findIndex((p) => p.visitNumber === visit.visitNumber);
    if (i >= 0) occupancy.splice(i, 1);

    auditEntries.unshift({
      id: Math.max(...auditEntries.map((a) => a.id)) + 1,
      userName: currentUser.username,
      action: 'CHECK-OUT',
      entityName: 'Visit',
      recordRef: visit.visitNumber,
      oldValue: 'Inside',
      newValue: 'CheckedOut',
      timestampUtc: visit.outTimeUtc,
      ipAddress: '10.20.4.9',
      deviceId: 'PORTAL',
    });

    return {
      outTimeUtc: visit.outTimeUtc,
      durationMinutes: Math.round(
        (Date.parse(visit.outTimeUtc) - Date.parse(visit.inTimeUtc ?? visit.outTimeUtc)) / 60000),
      status: 'CheckedOut',
    };
  },
};

let visitCounter = 1250;
function nextVisitNumber(): string {
  visitCounter += 1;
  return `VIS-${new Date().getFullYear()}-${String(visitCounter).padStart(8, '0')}`;
}

/** Mirrors the server's masking so the mock shows what the API would return. */
function maskEmiratesId(value: string): string {
  const digits = value.replace(/\D/g, '');
  return digits.length === 15 ? `${digits.slice(0, 3)}-XXXX-XXXXXXX-${digits.slice(-1)}` : 'XXXXXXXXXXX';
}

export type ApiClient = typeof mockClient;
