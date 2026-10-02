# SLA & Support Ownership

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — SLA & Support Ownership (Controlled, required · IT)

**Every `[ ]` in this document is a commitment somebody has to make.** It is written with
the shape filled in and the commitments blank, because an SLA invented by the person who
built the system is not an SLA.

---

## 1 · Who owns it

| Role | Name | Contact | Hours |
|---|---|---|---|
| Business Owner | `[ ]` | `[ ]` | |
| IT Owner (application) | `[ ]` | `[ ]` | |
| First-line support | `[ ]` | `[ ]` | `[ ]` |
| Second-line (application) | `[ ]` | `[ ]` | `[ ]` |
| Database | `[ ]` | `[ ]` | |
| Network / infrastructure | `[ ]` | `[ ]` | |
| ICP toolkit and licence | `[ ]` | ICP: `[ ]` | ICP's own hours |
| Out of hours | `[ ]` | `[ ]` | |

## 2 · Service hours

| | |
|---|---|
| Reception operating hours | `[ ]` |
| Supported hours | `[ ]` |
| Out-of-hours cover | `[ ]` Yes / No |
| Planned maintenance window | `[ ]` |

> Reception hours are the number that matters. The application only has to be up when
> somebody is standing at the desk.

## 3 · Severity

Severity is about **whether reception can work**, not about how technical the fault is.

| | Definition | Example | Response | Resolution |
|---|---|---|---|---|
| **S1 — Critical** | Nobody can be checked in by any means | The application is down; the database is unreachable | `[ ]` | `[ ]` |
| **S2 — High** | Check-in works but a path has failed, or the record is at risk | Every card read fails and reception is typing everything; the report cannot be read | `[ ]` | `[ ]` |
| **S3 — Medium** | A workaround exists and is in use | One tablet will not read cards (R-08); the camera scanner is unreliable | `[ ]` | `[ ]` |
| **S4 — Low** | Cosmetic or an improvement | Wording, layout, a column order | `[ ]` | `[ ]` |
| **S0 — Security** | Suspected exposure of visitor data or a credential | A lost tablet; a credential in a file; an unexplained export | **Immediate** | `19-incident-response.md` |

> **A failing card reader is S2, not S1.** Reception can type the visitor's details in, and
> that path has the same validation and the same record. Treating it as S1 would mean
> treating something with a working fallback as an emergency, and would make S1 mean
> nothing.
>
> **S0 is not a severity above S1 — it is a different route.** It goes to incident
> response, not to the support queue.

## 4 · Raising something

| Route | For |
|---|---|
| `[ ]` service desk | Everything except S0 |
| `[ ]` direct contact | S1, out of hours |
| `[ ]` security contact | S0, immediately, no queue |

Include: what you were doing, what the screen said **word for word**, which desk or tablet,
and the time. If there is a reference number on the screen, include that.

The messages in `15-user-manual.md` §3 are the ones reception will be reading out. They are
specific on purpose — *"The visit was not saved"* and *"The card read was not accepted"* are
different faults and the distinction saves a call.

## 5 · What support covers

**In scope**

- The application, the API and the Android app
- The database and its backups
- Entra ID role assignments
- The ICP toolkit integration, the desk agent service, and the tablet readers
- Card-reading faults, including liaison with ICP

**Out of scope**

- Reception PC hardware and the Windows build — standard IT support
- The office network and wifi — standard IT support
- **What ICP's Validation Gateway does.** IT can raise it; IT cannot fix it
- Whether a presented card is genuine. The system reports what the chip says

## 6 · Things that will be reported and are known

Reception will raise these. Each has an answer that is not "we will fix it":

| Reported as | Actually | Answer |
|---|---|---|
| "The card reader is broken" on one tablet | R-08, toolkit error 233 | Known. Use another tablet or type it in. `16-admin-guide.md` §8 |
| "It says the card expired but we need them in" | A warning, not a refusal | Save it. The warning is information |
| "It will not accept the ID number" | The check digit failed | Read the number off the card again. One digit is wrong |
| "It lost the card read" | A *field* was refused, not the read | The read is still there. Fill the field in |
| "Visitors are still showing as in the building" | Nothing signs out automatically | Sign them out. This is deliberate |

## 7 · Availability

| | |
|---|---|
| Target availability during reception hours | `[ ]` |
| Measured by | `GET /health` |
| Excluded | Planned maintenance; ICP gateway outages; office network outages |
| Reported | `[ ]` monthly / quarterly, to `[ ]` |

**An ICP outage is excluded deliberately.** It stops card reading and it does not stop
check-in — reception types the details in — so it is not an availability failure of this
application.

## 8 · Changes after go-live

No change is made directly (§7.2). Every change — of any size — is a Change Request work
item, a feature branch, a pull request reviewed by IT, a pipeline run, and the approval gate
in `14-go-live-approval.md`.

| Change type | Route | Lead time |
|---|---|---|
| Entity list | Script in `db/`, run by IT | `[ ]` |
| Role assignment | Entra, by IT | `[ ]` |
| Bug fix | Change Request → PR → pipeline → approval | `[ ]` |
| New feature | Change Request → the full §4 pipeline | `[ ]` |

## 9 · Review

This document is reviewed `[ ]` and whenever an owner changes.

| | Name | Signature | Date |
|---|---|---|---|
| Agreed — IT Head | | | |
| Agreed — Business Owner | | | |
