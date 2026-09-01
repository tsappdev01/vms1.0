# API Specification

`DI.Vms.Api`, ASP.NET Core 8. Base path `/api/v1`. JSON, TLS only.

Auth: Microsoft Entra ID bearer tokens (BRD §23). Reception devices additionally present a
client certificate (BRD §22, "device authentication"). Every mutating call is audited (§19).

## Registration and check-in

### `POST /api/v1/visits/identify`
Resolve a scanned ID to an existing visitor — repeat-visitor recognition (BRD §14).

The client sends fields **already extracted from the chip**; the server never sees the card.

```jsonc
{
  "idType": "EmiratesId",
  "idNumber": "784XXXXXXXXXXXX",
  "captureMethod": "CardReader",
  "cardVerification": {              // from IsCardGenuine / CheckCardStatus
    "isGenuine": true,
    "cardStatus": "Valid",
    "verifiedAtUtc": "2026-09-01T05:40:11Z",
    "vgAvailable": true
  }
}
```

```jsonc
// 200 - known visitor (BRD 14: "Existing Visitor Found")
{
  "found": true,
  "visitor": {
    "id": "…", "name": "Ahmed Khan", "company": "ABC Trading LLC",
    "idNumberMasked": "784-XXXX-XXXXXXX-1",
    "idExpiryDate": "2028-04-15", "idExpired": false,
    "totalVisits": 17, "lastVisitDate": "2026-08-28"
  }
}
// 200 - new visitor
{ "found": false }
```

`vgAvailable: false` is accepted: the visit is recorded `VerificationPending` rather than
refused. See the degraded-mode note in `01-architecture.md`.

### `POST /api/v1/visits`
Create the visit (BRD §4). Returns `Expected`; no in-time yet.

```jsonc
{
  "visitor": {                        // omit if visitorId supplied
    "name": "Ahmed Khan", "company": "ABC Trading LLC",
    "idType": "EmiratesId", "idNumber": "784XXXXXXXXXXXX",
    "idExpiryDate": "2028-04-15", "nationality": "PAK",
    "dateOfBirth": "1985-03-22", "photo": "<base64>",
    "captureMethod": "CardReader"
  },
  "visitorId": null,
  "diEntityId": "…", "hostEmployeeId": "…",
  "purpose": "Business Meeting", "visitType": "Guest"
}
```

`floor`, `office` and `department` are **not** accepted from the client — the server
snapshots them from the host record, so they cannot be spoofed or drift.

### `POST /api/v1/visits/{id}/check-in`
BRD §7. Signature is mandatory.

```jsonc
{ "signatureImage": "<base64 png>", "deviceId": "RECEPTION-TABLET-01" }
```
```jsonc
{ "visitNumber": "VIS-2026-00001245", "inTimeUtc": "…", "status": "Inside",
  "host": { "name": "John Smith", "notified": true } }
```

`409` if already checked in. Triggers host notification (§8) and, if enabled, the badge (§9).

### `POST /api/v1/visits/{id}/check-out`
BRD §10. `409` unless the visit is `Inside`.

```jsonc
{ "outTimeUtc": "…", "durationMinutes": 96, "status": "CheckedOut" }
```

## Dashboard and emergency

| Endpoint | Purpose | BRD |
|---|---|---|
| `GET /api/v1/dashboard/summary` | Total today / inside / checked out / expected | §11 |
| `GET /api/v1/visits/inside` | Currently inside, paged | §11 |
| `GET /api/v1/emergency/occupancy` | **Everyone on site**, evacuation list | §12 |
| `GET /api/v1/visits/expected?date=` | Expected visitors | §16 |

`/emergency/occupancy` is unpaged, served from cache, and available to **every**
authenticated role including Security Officer. In an evacuation nobody should be
blocked by pagination or a permission check.

Live updates over SignalR hub `/hubs/dashboard`: `VisitCheckedIn`, `VisitCheckedOut`,
`OccupancyChanged`.

## Search, history, masters, reports

| Endpoint | Purpose | BRD |
|---|---|---|
| `GET /api/v1/visitors/search?q=&idNumber=&company=&host=&from=&to=&status=&floor=` | Multi-criterion search | §13 |
| `GET /api/v1/visitors/{id}/history` | Visitor profile and previous visits | §13 |
| `GET /api/v1/employees/search?q=` | Host lookup, auto-populates dept/floor/office | §5 |
| `GET|POST|PUT /api/v1/entities` | DI entity master | §6 |
| `GET|POST|PUT /api/v1/employees` | Host master | §5 |
| `GET|POST|PUT /api/v1/buildings\|floors\|offices` | Location masters | §17 |
| `GET|POST|PUT /api/v1/users` | Users and roles | §18 |
| `GET /api/v1/audit?entity=&recordId=&userId=&from=&to=` | Audit trail | §19 |
| `GET /api/v1/reports/{name}` | The §20 report set, `?format=json\|csv\|xlsx` | §20 |

Searching by `idNumber` hashes the input and matches `IdNumberHash` — the plaintext is
never compared and never logged.

## Cross-cutting

**ID masking (§22).** `idNumber` is never returned. Responses carry `idNumberMasked`
(`784-XXXX-XXXXXXX-1`). A user with `CanViewUnmaskedId` may call
`GET /api/v1/visitors/{id}/id-number`, which returns the full value **and writes a
`VIEW-UNMASKED-ID` audit row**. Looking at an ID number is itself an auditable event.

**Errors.** RFC 7807 `application/problem+json`. Domain rule violations → `409` with a
machine-readable `code`. Validation → `400` with per-field detail.

**Idempotency.** `POST /visits` and both transition endpoints accept `Idempotency-Key`.
Reception has flaky wifi; a retried check-in must not create a second visit.

**Paging.** `?page=&pageSize=` (default 25, max 200), envelope `{ items, page, pageSize, totalCount }`.

## Authorisation matrix (BRD §18)

| Endpoint group | Officer | Supervisor | Admin | SysAdmin |
|---|:--:|:--:|:--:|:--:|
| Register, check-in, check-out | ✅ | ✅ | ✅ | ✅ |
| Search, current visitors | ✅ | ✅ | ✅ | ✅ |
| Emergency occupancy | ✅ | ✅ | ✅ | ✅ |
| Correct/amend a visit | ❌ | ✅ | ✅ | ✅ |
| Reports | ❌ | ✅ | ✅ | ✅ |
| Masters (entities, hosts, locations) | ❌ | ❌ | ✅ | ✅ |
| Users and roles | ❌ | ❌ | ✅ | ✅ |
| Unmasked ID number | ❌ | permission | permission | permission |
| Audit log | ❌ | read | read | read |
| System configuration, integrations | ❌ | ❌ | ❌ | ✅ |

`CanViewUnmaskedId` is a per-user grant, not implied by any role — §22 requires that
normal Security users see only the masked form, and seniority is not the same as need.
