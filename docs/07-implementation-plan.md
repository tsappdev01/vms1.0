# Implementation Plan

Follows BRD §25, adjusted for what the SDK analysis found.

## Phase 0 — Prerequisites (blocking)

None of the ID-reading work can start until these are settled. They are procurement and
compliance items, not development, so they should start immediately and in parallel.

| Item | Owner | Blocks |
|---|---|---|
| ICP **Service Provider licence** confirmed | DI / ICP | All chip reading |
| **Reader spike**: read a real card with the Windows sample | DI | Everything downstream |
| **Reader hardware** confirmed (PC/SC on Windows; plugin-matched on mobile) | DI / vendor | Reception app |
| **Production** config bundle + production `vg_url` | ICP | Go-live |
| VG reachable from DIP network (firewall) | DI IT | Verification calls |
| ~~iOS toolkit exists?~~ **Resolved: yes** | — | Client platform now an open choice, not a constraint |
| **Retention period** defined | DI Legal | Retention job |

## Phase 1 — Core Visitor Management

Tablet app, ID scanning, registration, entity/host, signature, check-in/out, database,
dashboard, basic reports, user management, audit trail.

Added on the strength of the SDK analysis:
- **`IsCardGenuine` and `CheckCardStatus`** in Phase 1, not later. They are the difference
  between a visitor log and identity verification, and BRD §26's stated principle is
  identity *verification*. The capability is already in the SDK being paid for.
- **Emergency occupancy screen** in Phase 1. BRD §12 calls it mandatory; it is listed under
  Phase 3 in §25. Occupancy data already exists from day one, so the screen is cheap now
  and its absence is a genuine safety gap.
- Device registration and licence-expiry monitoring.

## Phase 2 — Smart Visitor Management

Pre-registration and QR, host email/Teams notification, visitor badge, appointments,
repeat-visitor recognition, expected-visitor dashboard, advanced reports, Power BI.

Host notification (§8) is small and disproportionately valuable — bring it forward into
late Phase 1 if the schedule allows.

## Phase 3 — Building Security Integration

Access control, turnstiles, barriers, CCTV, employee directory, Teams notification,
digital visitor pass, multi-building.

Biometric verification (`AuthenticateBiometricOnCard` / `OnServer`) belongs here — the SDK
supports it and it suits contractors and high-security areas.

## Sequencing within Phase 1

1. Domain + schema + EF Core against `UATWEB01/VMS` ← *in progress*
2. API: masters, then registration/check-in/check-out
3. Reception app shell + auth + device registration
4. **ID read spike against real hardware** — do this early; it carries the most unknowns
5. Signature, check-in/out, offline outbox
6. Portal: dashboard, current visitors, emergency
7. Search, history, reports
8. Audit, retention job, hardening
9. UAT

Step 4 should not wait for steps 2–3. It is the only part of the system whose feasibility
is not yet proven end to end, and it depends on hardware that has not been chosen.

## Status

| Item | State |
|---|---|
| SDK analysis | ✅ |
| Architecture, data model, API spec, client designs, security | ✅ |
| Domain model | ✅ `src/DI.Vms.Domain` |
| SQL schema | ✅ `db/schema` |
| Application / Infrastructure / API projects | ⬜ |
| Reception app | ⬜ blocked on Phase 0 hardware |
| Web portal | ⬜ |
