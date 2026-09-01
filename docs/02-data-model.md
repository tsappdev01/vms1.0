# Data Model

Implements BRD §17, with the privacy controls of §22 applied. DDL: `db/schema/`.
Domain types: `src/DI.Vms.Domain/`.

## Entities

| Table | Purpose | BRD |
|---|---|---|
| `Visitor` | A person, identified once, reused across visits | §17, §14 |
| `Visit` | One arrival: the full chain of custody | §17, §26 |
| `VisitorSignature` | Acknowledgement drawn at check-in | §7 |
| `Employee` | Host master — search target for "Person to Visit" | §5 |
| `DiEntity` | DI group companies | §6 |
| `Building` / `Floor` / `Office` | Location hierarchy | §17 |
| `User` | Operators and their roles | §17, §18 |
| `AuditLog` | Append-only change record | §19 |

## Visitor — identity, stored minimally

BRD §3 recommends capturing only what visitor management needs rather than the whole ID
document. The chip read makes that easy: take the fields, never store a document image.

| Column | Note |
|---|---|
| `IdNumberCipher` | `VARBINARY(512)`. Ciphertext. Never stored in clear. |
| `IdNumberHash` | `BINARY(32)`. HMAC-SHA256 under a server-side pepper. |
| `IdExpiryDate` | Drives the Expired ID Report (§20). |
| `Photo` | Optional, subject to policy (§3). Portrait only — never a full document scan. |
| `CaptureMethod` | `Manual`/`Ocr`/`Mrz`/`Nfc`/`BarcodeOrQr`/`CardReader` — audit of *how* identity was established. |
| `PurgedAtUtc` | Set by the retention job (§22). |

`UNIQUE (IdType, IdNumberHash)` is what makes repeat-visitor recognition (§14) a single
indexed lookup with no decryption. The pepper lives in Key Vault, not in the database —
otherwise a database copy alone would let an attacker confirm whether a known person ever
visited.

## Visit — the chain of custody

BRD §26: *Identity → Company → DI Entity → Host → Purpose → Check-In → Location → Signature → Check-Out.*

`Floor`, `Office` and `Department` are **snapshotted from the host at registration**. If the
host later moves desks, historical visits must still show where the visitor actually went —
an evacuation record that silently rewrites itself is worse than useless.

### Lifecycle

```
Expected ──check-in──► Inside ──check-out──► CheckedOut
   │                      │
   └──(no-show, nightly)──┴──(never checked out, nightly)──► Cancelled / AutoClosed
```

Enforced in two places: `Visit.CheckIn()` / `Visit.CheckOut()` in the domain, and
`CK_Visit_Times` in the database. The check constraint means a bug in the application
cannot leave a row claiming to be `Inside` with no in-time.

`AutoClosed` marks visits the nightly job closed, so the "Visitors Never Checked Out"
report (§20) stays honest instead of being quietly cleaned up.

## Visit numbering

`VIS-2026-00001245` (§7): `VIS-{year}-{8-digit sequence}`, via `vms.VisitNumberSequence`
and `vms.NextVisitNumber`. Allocated at check-in, not at pre-registration, so numbers
track actual arrivals.

## Indexing

Driven by the queries the BRD actually asks for:

| Index | Serves |
|---|---|
| `IX_Visit_Inside` (filtered `Status = 2`) | Dashboard "currently inside" (§11), evacuation list (§12) |
| `IX_Visit_Visitor_InTime` | Visitor history (§13) |
| `IX_Visit_Host_InTime` | Visitor-by-host report (§20) |
| `IX_Visit_Expected` (filtered) | Expected visitors (§16) |
| `IX_Employee_Name_Active` (filtered) | Host search on the tablet (§5) |
| `IX_Visitor_IdExpiry` | Expired ID report (§20) |

The evacuation list (§12) is the one query that must never be slow — it is used when the
building is being emptied. Filtered index, covering, and served from a SignalR-pushed
cache rather than a fresh query per refresh.

## Deliberate deviations from BRD §17

| BRD | Here | Why |
|---|---|---|
| `Visitor.IDNumber` plain | `IdNumberCipher` + `IdNumberHash` | §22 requires encryption at rest; hash preserves §14 lookup |
| `Visit.Signature` inline | `VisitorSignature` table | Keeps image bytes off every visit query |
| `Employee.Floor`/`Office` as text | FK to `Floor`/`Office` | §17 defines both as entities; text would desync from the master |
| — | `Visit.AutoClosed` | Needed for the §20 "never checked out" report |
| — | `Visitor.CaptureMethod` | §21 leaves the method open, so record which was used |
| — | `User.CanViewUnmaskedId` | §22 masking needs an explicit permission to unmask |
