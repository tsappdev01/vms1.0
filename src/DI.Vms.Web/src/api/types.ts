/* DTOs mirroring docs/03-api-specification.md.
   The raw ID number never appears in a list or detail response - only the masked
   form. It is available solely from the dedicated, permission-gated and audited
   endpoint (see fetchUnmaskedId). */

export type IdType =
  | 'EmiratesId'
  | 'Passport'
  | 'UaeDrivingLicence'
  | 'GccId'
  | 'OtherGovernmentId';

export type VisitStatus = 'Expected' | 'Inside' | 'CheckedOut' | 'Cancelled';

export type VerificationState =
  | 'Verified'
  | 'IdExpired'
  | 'Failed'
  | 'Pending';

export type VisitorType =
  | 'Guest' | 'Customer' | 'Supplier' | 'Contractor' | 'Consultant'
  | 'GovernmentOfficial' | 'InterviewCandidate' | 'Delivery'
  | 'ServiceProvider' | 'Other';

export type UserRole =
  | 'SecurityOfficer' | 'SecuritySupervisor' | 'Admin' | 'SystemAdministrator';

export interface DashboardSummary {
  totalToday: number;
  currentlyInside: number;
  checkedOut: number;
  expected: number;
  visitorsByEntity: { entityName: string; visitors: number }[];
}

export interface VisitListItem {
  id: string;
  visitNumber: string;
  visitorName: string;
  company: string | null;
  idType: IdType;
  idNumberMasked: string;
  idExpired: boolean;
  hostName: string;
  entityName: string;
  department: string | null;
  floor: string | null;
  office: string | null;
  purpose: string | null;
  visitType: VisitorType;
  inTimeUtc: string | null;
  outTimeUtc: string | null;
  expectedDate: string | null;
  expectedTime: string | null;
  status: VisitStatus;
  verification: VerificationState;
}

export interface OccupancyPerson {
  name: string;
  company: string | null;
  hostName: string | null;
  floor: string | null;
  category: 'Visitor' | 'Contractor' | 'ServiceProvider' | 'Employee';
  inTimeUtc: string | null;
  visitNumber: string | null;
}

export interface VisitorProfile {
  id: string;
  name: string;
  company: string | null;
  idType: IdType;
  idNumberMasked: string;
  idExpiryDate: string | null;
  idExpired: boolean;
  nationality: string | null;
  totalVisits: number;
  lastVisitDate: string | null;
  visits: VisitListItem[];
}

export interface DiEntityDto {
  id: string;
  entityCode: string;
  entityName: string;
  isActive: boolean;
}

export interface EmployeeDto {
  id: string;
  employeeCode: string;
  name: string;
  entityName: string;
  department: string | null;
  designation: string | null;
  floor: string | null;
  office: string | null;
  email: string | null;
  mobile: string | null;
  isActive: boolean;
}

export interface UserDto {
  id: string;
  username: string;
  name: string;
  role: UserRole;
  securityLocation: string | null;
  canViewUnmaskedId: boolean;
  isActive: boolean;
}

export interface AuditEntry {
  id: number;
  userName: string;
  action: string;
  entityName: string;
  recordRef: string | null;
  oldValue: string | null;
  newValue: string | null;
  timestampUtc: string;
  ipAddress: string | null;
  deviceId: string | null;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface VisitorSearchQuery {
  q?: string;
  company?: string;
  host?: string;
  status?: VisitStatus | '';
  floor?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
