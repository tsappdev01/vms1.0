# Go-Live Approval

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Go-Live Approval (Controlled: **IT Head + Business Owner**)

The release pipeline's environment approval gate (§7.1). Nothing reaches production until
this is signed.

| | |
|---|---|
| Application | Visitor Management System (VMS) |
| Classification | Controlled |
| Version | `[ ]` web commit · `[ ]` Android versionName (versionCode) |
| Target host | `[ ]` UATWEB01 / Azure Web App / both |
| Proposed go-live date | `[ ]` |

---

## 1. Documentation gate (§6, §10)

No application handling employee, financial or customer data goes live without the
documentation set for its classification. All twenty are required for Controlled.

| # | Document | Present | Complete |
|---|---|:-:|:-:|
| 1 | System Architecture Document | ✅ | ⬜ |
| 2 | Data Flow Diagram | ✅ | ⬜ |
| 3 | Database Schema / Data Dictionary | ✅ *(generated)* | ⬜ |
| 4 | API Documentation | ✅ | ⬜ |
| 5 | Deployment & Infrastructure Guide | ✅ | ⬜ |
| 6 | README | ✅ | ⬜ |
| 7 | Security Review Checklist | ✅ | ⬜ **requires §E executed** |
| 8 | Data Classification Statement | ✅ | ⬜ **requires the retention decision** |
| 9 | Access Control Matrix | ✅ | ⬜ |
| 10 | Risk Assessment | ✅ | ⬜ |
| 11 | IT Asset Registration | ✅ | ⬜ **requires owners named** |
| 12 | UAT Test Cases | ✅ | ⬜ **requires execution** |
| 13 | UAT Sign-off Sheet | ✅ | ⬜ **requires signatures** |
| 14 | Go-Live Approval | *this document* | ⬜ |
| 15 | User Manual | ✅ | ⬜ **requires screenshots** |
| 16 | Admin Guide | ✅ | ⬜ |
| 17 | Backup & Recovery Procedure | ✅ | ⬜ **requires RPO/RTO decided** |
| 18 | SLA & Support Ownership | ✅ | ⬜ **requires hours and owners decided** |
| 19 | Incident Response Procedure | ✅ | ⬜ **requires contacts named** |
| 20 | Change Log / Version History | ✅ | ⬜ |

## 2. Technical gate

Each is a yes/no with evidence, not a judgement call.

| # | Condition | Evidence | Met |
|---|---|---|:-:|
| T1 | `Authentication:Enabled` is **`true`** in the production configuration | Screenshot of the setting | ⬜ |
| T2 | Entra ID app roles exist and are assigned to real people | Entra app-role assignment export | ⬜ |
| T3 | **All four development credentials have been rotated** — Entra client secret, SQL password, storage AccountKey, API key | Rotation record | ⬜ |
| T4 | No credential is present in `appsettings.json` or `appsettings.Development.json` | CI secret-scan job summary, clean | ⬜ |
| T5 | Each tablet has its **own** API key | Tablet settings, three distinct keys | ⬜ |
| T6 | Every tablet's settings screen is behind a PIN | Tested on each device | ⬜ |
| T7 | HTTPS with a valid certificate on the production host | Browser inspection | ⬜ |
| T8 | Security headers present | `07-…` §E15 | ⬜ |
| T9 | Rate limiting active on the internet-facing host | `07-…` §E14 | ⬜ |
| T10 | `/health` returns healthy against the production database | Request log | ⬜ |
| T11 | All `db/` scripts have been applied to the production database | `db/apply.cmd` output | ⬜ |
| T12 | Startup log shows the bootstrapper's decisions and no refusal | Startup log | ⬜ |
| T13 | The entity list in production is correct and current | Reviewed by the Business Owner | ⬜ |
| T14 | A backup of the production database has been taken **and a restore tested** | `17-backup-and-recovery.md` | ⬜ |
| T15 | OWASP ZAP baseline scan: no High findings | `07-…` §E19 | ⬜ |
| T16 | Branch protection: PR required, one IT reviewer, direct commits to main blocked | Repository settings | ⬜ |
| T17 | ICP Service Provider licence expiry is recorded and diarised 60 days ahead | `11-it-asset-registration.md` | ⬜ |
| T18 | Entra client secret expiry is recorded and diarised 60 days ahead | `11-it-asset-registration.md` | ⬜ |

> **T3 is the one to refuse over.** Four credentials were exposed during development and one
> SQL password is permanently in the repository's git history. Going live without rotating
> them puts a known-exposed credential in front of real visitor data.

## 3. Operational gate

| # | Condition | Met |
|---|---|:-:|
| O1 | Reception has been trained; `15-user-manual.md` is at the desk | ⬜ |
| O2 | Support ownership and hours are agreed and published (`18-sla-and-support.md`) | ⬜ |
| O3 | The incident contacts in `19-incident-response.md` are real people who know they are on it | ⬜ |
| O4 | The manual-entry fallback has been demonstrated to reception, so a dead reader is a known situation rather than a crisis | ⬜ |
| O5 | A rollback has been rehearsed: previous web build and previous APK, both available and installable | ⬜ |

## 4. Deviations requiring Policy Owner approval

These are departures from DI-IT-POL-AIDEV-001 that the application will go live with. Each
needs **written** approval; an unsigned row blocks go-live.

| # | Policy clause | Deviation | Why | Approved by | Date |
|---|---|---|---|---|---|
| D1 | §4.4, §7 — Azure DevOps repository | GitHub (`tsappdev01/vms1.0`) with branch protection, PR review and Actions pipelines | The organisation's repositories for this work are on GitHub | | |
| D2 | §4.4 — Audit trail table in MSSQL with old and new values | No audit table. Visits cannot be edited or deleted through the application, so the row is its own record | There are no updates or deletes to audit. The real gap is *reads*, carried as R-04 | | |
| D3 | §4.4 — Input validation using FluentValidation | Server-side validation in `Services/VisitorFields.cs`, one definition used by the desk, the tablet and the API | Functionally equivalent; FluentValidation is approved as a follow-up | | |
| D4 | §4.4 — Roles Viewer / User / Admin | Officer / Supervisor / Admin / SystemAdmin / UnmaskedId | Finer-grained and named for the actual job. `UnmaskedId` exists because seniority is not a need to see an Emirates ID number | | |
| D5 | §4.4 — IIS on Windows Server, no additional runtime dependencies | ICP toolkit and the desk agent service are required on reception PCs | A chip cannot be read without them | | |
| D6 | §7 — Semgrep SAST on every merge | Not yet implemented | Carried as R-07, to be added | | |
| D7 | Card images in Azure Blob Storage | Where a storage account is configured | | | |

## 5. Decision

- [ ] **Approved for go-live** — every gate above is met, or carries a signed deviation
- [ ] **Approved with conditions** — stated below, with dates
- [ ] **Not approved** — reasons stated below

Conditions or reasons:

```
```

| Role | Name | Signature | Date |
|---|---|---|---|
| **IT Head** | | | |
| **Business Owner** | | | |
| **Policy Owner** (for §4 deviations) | | | |

---

After go-live, **no further change is made directly** (§7.2). Every change — of any size —
is a Change Request work item, a feature branch, a pull request reviewed by IT, a pipeline
run, and this same approval gate.
