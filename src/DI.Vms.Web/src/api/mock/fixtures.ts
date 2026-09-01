/* In-memory fixtures so the portal is reviewable before DI.Vms.Api exists.
   Names and reference numbers follow the worked examples in the BRD. */

import type {
  AuditEntry, DiEntityDto, EmployeeDto, UserDto, VisitListItem, OccupancyPerson,
} from '../types';

const today = new Date();

/** An ISO timestamp for today at the given Gulf Standard Time wall clock. */
function gstToday(hour: number, minute: number): string {
  const d = new Date(Date.UTC(
    today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate(),
    hour - 4, minute, 0,
  ));
  return d.toISOString();
}

function daysAgo(n: number): string {
  const d = new Date(today);
  d.setUTCDate(d.getUTCDate() - n);
  return d.toISOString().slice(0, 10);
}

export const entities: DiEntityDto[] = [
  { id: 'e1', entityCode: 'DIPJSC', entityName: 'Dubai Investments PJSC', isActive: true },
  { id: 'e2', entityCode: 'DIP', entityName: 'Dubai Investments Park', isActive: true },
  { id: 'e3', entityCode: 'NGI', entityName: 'National General Insurance', isActive: true },
  { id: 'e4', entityCode: 'MSH', entityName: 'Masharie', isActive: true },
  { id: 'e5', entityCode: 'GLASS', entityName: 'Glass Entities', isActive: true },
  { id: 'e6', entityCode: 'OTHER', entityName: 'Other DI Subsidiaries', isActive: true },
];

export const employees: EmployeeDto[] = [
  { id: 'h1', employeeCode: 'DI-1042', name: 'John Smith', entityName: 'Dubai Investments PJSC', department: 'Finance', designation: 'Finance Manager', floor: '8', office: '801', email: 'john.smith@example.ae', mobile: '+971 50 000 0001', isActive: true },
  { id: 'h2', employeeCode: 'DI-1088', name: 'Sarah Ahmed', entityName: 'Dubai Investments PJSC', department: 'Legal', designation: 'Legal Counsel', floor: '5', office: '512', email: 'sarah.ahmed@example.ae', mobile: '+971 50 000 0002', isActive: true },
  { id: 'h3', employeeCode: 'DI-1190', name: 'Peter Thomas', entityName: 'Masharie', department: 'Operations', designation: 'Operations Head', floor: '12', office: '1203', email: 'peter.thomas@example.ae', mobile: '+971 50 000 0003', isActive: true },
  { id: 'h4', employeeCode: 'NGI-233', name: 'Fatima Al Marri', entityName: 'National General Insurance', department: 'Claims', designation: 'Claims Supervisor', floor: '3', office: '305', email: 'fatima.almarri@example.ae', mobile: '+971 50 000 0004', isActive: true },
];

function visit(o: Partial<VisitListItem> & Pick<VisitListItem, 'id' | 'visitNumber' | 'visitorName' | 'status'>): VisitListItem {
  return {
    company: null, idType: 'EmiratesId', idNumberMasked: '784-XXXX-XXXXXXX-1',
    idExpired: false, hostName: 'John Smith', entityName: 'Dubai Investments PJSC',
    department: 'Finance', floor: '8', office: '801', purpose: 'Business Meeting',
    visitType: 'Guest', inTimeUtc: null, outTimeUtc: null, expectedDate: null,
    expectedTime: null, verification: 'Verified',
    ...o,
  };
}

export const visits: VisitListItem[] = [
  visit({ id: 'v1', visitNumber: 'VIS-2026-00001245', visitorName: 'Ahmed Khan', company: 'ABC Trading LLC', status: 'Inside', inTimeUtc: gstToday(9, 42) }),
  visit({ id: 'v2', visitNumber: 'VIS-2026-00001246', visitorName: 'Ali Raza', company: 'XYZ LLC', hostName: 'Sarah Ahmed', department: 'Legal', floor: '5', office: '512', status: 'Inside', inTimeUtc: gstToday(10, 12), idNumberMasked: '784-XXXX-XXXXXXX-7' }),
  visit({ id: 'v3', visitNumber: 'VIS-2026-00001247', visitorName: 'Mohammed Ali', company: 'Gulf Facilities', hostName: 'Peter Thomas', entityName: 'Masharie', department: 'Operations', floor: '12', office: '1203', visitType: 'Contractor', purpose: 'Maintenance', status: 'Inside', inTimeUtc: gstToday(8, 5), idNumberMasked: '784-XXXX-XXXXXXX-3' }),
  visit({ id: 'v4', visitNumber: 'VIS-2026-00001248', visitorName: 'Priya Nair', company: 'Deloitte', hostName: 'Sarah Ahmed', department: 'Legal', floor: '5', office: '512', visitType: 'Consultant', purpose: 'Audit', status: 'Inside', inTimeUtc: gstToday(11, 3), idNumberMasked: 'XXXXXXXX4412', idType: 'Passport', verification: 'Pending' }),
  visit({ id: 'v5', visitNumber: 'VIS-2026-00001249', visitorName: 'Rashid Al Hosani', company: 'Ministry of Economy', hostName: 'John Smith', visitType: 'GovernmentOfficial', purpose: 'Official Visit', status: 'Inside', inTimeUtc: gstToday(11, 40), idNumberMasked: '784-XXXX-XXXXXXX-9', idExpired: true, verification: 'IdExpired' }),
  visit({ id: 'v6', visitNumber: 'VIS-2026-00001240', visitorName: 'Elena Petrova', company: 'Siemens', hostName: 'Peter Thomas', entityName: 'Masharie', floor: '12', office: '1203', status: 'CheckedOut', inTimeUtc: gstToday(9, 0), outTimeUtc: gstToday(10, 36), idNumberMasked: 'XXXXXXXX8821', idType: 'Passport' }),
  visit({ id: 'v7', visitNumber: 'VIS-2026-00001241', visitorName: 'Omar Farouk', company: 'Emirates NBD', hostName: 'Fatima Al Marri', entityName: 'National General Insurance', department: 'Claims', floor: '3', office: '305', visitType: 'Customer', status: 'CheckedOut', inTimeUtc: gstToday(8, 30), outTimeUtc: gstToday(9, 15), idNumberMasked: '784-XXXX-XXXXXXX-2' }),
  visit({ id: 'v8', visitNumber: 'VIS-2026-00001242', visitorName: 'Grace Mensah', company: 'DHL', hostName: 'John Smith', visitType: 'Delivery', purpose: 'Document Delivery', status: 'CheckedOut', inTimeUtc: gstToday(7, 55), outTimeUtc: gstToday(8, 5), idNumberMasked: '784-XXXX-XXXXXXX-6' }),
  visit({ id: 'v9', visitNumber: '', visitorName: 'Daniel Osei', company: 'KPMG', hostName: 'Sarah Ahmed', department: 'Legal', floor: '5', office: '512', visitType: 'Consultant', status: 'Expected', expectedDate: daysAgo(0), expectedTime: '14:30', idNumberMasked: 'Not yet captured' }),
  visit({ id: 'v10', visitNumber: '', visitorName: 'Aisha Rahman', company: 'Talent Partners', hostName: 'John Smith', visitType: 'InterviewCandidate', purpose: 'Interview', status: 'Expected', expectedDate: daysAgo(0), expectedTime: '15:00', idNumberMasked: 'Not yet captured' }),
];

export const occupancy: OccupancyPerson[] = visits
  .filter((v) => v.status === 'Inside')
  .map((v) => ({
    name: v.visitorName,
    company: v.company,
    hostName: v.hostName,
    floor: v.floor,
    category: v.visitType === 'Contractor' ? 'Contractor'
      : v.visitType === 'ServiceProvider' ? 'ServiceProvider' : 'Visitor',
    inTimeUtc: v.inTimeUtc,
    visitNumber: v.visitNumber,
  }));

export const users: UserDto[] = [
  { id: 'u1', username: 'security01', name: 'Yusuf Kamal', role: 'SecurityOfficer', securityLocation: 'DIP Main Reception', canViewUnmaskedId: false, isActive: true },
  { id: 'u2', username: 'security02', name: 'Noura Saleh', role: 'SecurityOfficer', securityLocation: 'DIP Gate 2', canViewUnmaskedId: false, isActive: true },
  { id: 'u3', username: 'supervisor01', name: 'Layla Haddad', role: 'SecuritySupervisor', securityLocation: 'DIP Main Reception', canViewUnmaskedId: true, isActive: true },
  { id: 'u4', username: 'admin01', name: 'Imran Qureshi', role: 'Admin', securityLocation: null, canViewUnmaskedId: false, isActive: true },
  { id: 'u5', username: 'sysadmin', name: 'Deepa Menon', role: 'SystemAdministrator', securityLocation: null, canViewUnmaskedId: true, isActive: true },
];

export const auditEntries: AuditEntry[] = [
  { id: 1008, userName: 'security01', action: 'CHECK-IN', entityName: 'Visit', recordRef: 'VIS-2026-00001245', oldValue: null, newValue: 'Inside', timestampUtc: gstToday(9, 42), ipAddress: '10.20.4.51', deviceId: 'RECEPTION-TABLET-01' },
  { id: 1009, userName: 'security01', action: 'CHECK-IN', entityName: 'Visit', recordRef: 'VIS-2026-00001246', oldValue: null, newValue: 'Inside', timestampUtc: gstToday(10, 12), ipAddress: '10.20.4.51', deviceId: 'RECEPTION-TABLET-01' },
  { id: 1010, userName: 'supervisor01', action: 'UPDATE-HOST', entityName: 'Visit', recordRef: 'VIS-2026-00001245', oldValue: 'John Smith', newValue: 'Peter Thomas', timestampUtc: gstToday(10, 5), ipAddress: '10.20.4.9', deviceId: 'PORTAL' },
  { id: 1011, userName: 'supervisor01', action: 'VIEW-UNMASKED-ID', entityName: 'Visitor', recordRef: 'Ahmed Khan', oldValue: null, newValue: null, timestampUtc: gstToday(10, 7), ipAddress: '10.20.4.9', deviceId: 'PORTAL' },
  { id: 1012, userName: 'security02', action: 'CHECK-OUT', entityName: 'Visit', recordRef: 'VIS-2026-00001240', oldValue: 'Inside', newValue: 'CheckedOut', timestampUtc: gstToday(10, 36), ipAddress: '10.20.4.52', deviceId: 'RECEPTION-TABLET-02' },
  { id: 1013, userName: 'admin01', action: 'CREATE', entityName: 'Employee', recordRef: 'DI-1190', oldValue: null, newValue: 'Peter Thomas', timestampUtc: gstToday(9, 20), ipAddress: '10.20.4.14', deviceId: 'PORTAL' },
];

/** Signed-in user, for the permission gating the portal demonstrates. */
export const currentUser: UserDto = users[2]!;
