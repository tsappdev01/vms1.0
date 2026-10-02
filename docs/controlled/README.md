# Controlled documentation set

**Visitor Management System · Dubai Investments PJSC**
The documentation package DI-IT-POL-AIDEV-001 §6 requires of a **Controlled** application.

VMS handles government-issued identity documents belonging to members of the public, and
employee directory data. Either alone makes it Controlled, so no borderline case is argued
anywhere in this set.

> **Nothing in here is signed.** The application has not gone live. Every governance
> document carries `[ ]` where a name, a date or a decision belongs, and those blanks are
> the work remaining — not an oversight in the writing.

---

## The twenty documents

| # | Document (policy §6) | Here | State |
|:-:|---|---|---|
| 1 | System Architecture Document | [`01-system-architecture.md`](01-system-architecture.md) | ✅ |
| 2 | Data Flow Diagram | [`02-data-flow.md`](02-data-flow.md) | ✅ |
| 3 | Database Schema / Data Dictionary | [`03-data-dictionary.md`](03-data-dictionary.md) | ✅ **generated** |
| 4 | API Documentation | [`04-api-reference.md`](04-api-reference.md) | ✅ |
| 5 | Deployment & Infrastructure Guide | [`05-infrastructure.md`](05-infrastructure.md) + [`../deployment.md`](../deployment.md), [`../azure-deployment.md`](../azure-deployment.md) | ✅ |
| 6 | README (build & run guide) | [`../../README.md`](../../README.md), [`../README.md`](../README.md) | ✅ |
| 7 | Security Review Checklist | [`07-security-review-checklist.md`](07-security-review-checklist.md) | ⚠️ §E to be executed by IT |
| 8 | Data Classification Statement | [`08-data-classification.md`](08-data-classification.md) | ⚠️ retention decision outstanding |
| 9 | Access Control Matrix | [`09-access-control-matrix.md`](09-access-control-matrix.md) | ✅ |
| 10 | Risk Assessment | [`10-risk-assessment.md`](10-risk-assessment.md) | ✅ 14 risks, 12 open |
| 11 | IT Asset Registration | [`11-it-asset-registration.md`](11-it-asset-registration.md) | ⚠️ owners and expiry dates to name |
| 12 | UAT Test Cases | [`12-uat-test-cases.md`](12-uat-test-cases.md) | ⚠️ 34 scenarios, none executed |
| 13 | UAT Sign-off Sheet | [`13-uat-signoff.md`](13-uat-signoff.md) | ⚠️ unsigned |
| 14 | Go-Live Approval | [`14-go-live-approval.md`](14-go-live-approval.md) | ⚠️ unsigned |
| 15 | User Manual | [`15-user-manual.md`](15-user-manual.md) | ⚠️ screenshots to take |
| 16 | Admin Guide | [`16-admin-guide.md`](16-admin-guide.md) | ✅ |
| 17 | Backup & Recovery Procedure | [`17-backup-and-recovery.md`](17-backup-and-recovery.md) | ⚠️ RPO/RTO to decide; restore to test |
| 18 | SLA & Support Ownership | [`18-sla-and-support.md`](18-sla-and-support.md) | ⚠️ hours and owners to agree |
| 19 | Incident Response Procedure | [`19-incident-response.md`](19-incident-response.md) | ⚠️ contacts to name |
| 20 | Change Log / Version History | [`20-change-log.md`](20-change-log.md) | ✅ |

**Also required by §4.6**, named in the Standard Documentation Prompt though not in the
§6 table:

| | Document | Here | State |
|:-:|---|---|---|
| 21 | Known Issues Log | [`21-known-issues.md`](21-known-issues.md) | ✅ 26 items, 10 open |

**Written: 21 of 21.** Nine need a decision, a signature, an execution or a screenshot from
somebody other than the author, and each says which.

## What this set replaced

Before it, seven of the twenty existed in some form and the rest did not:

| Was | Now |
|---|---|
| `docs/01-architecture.md` … `07-implementation-plan.md` | **Superseded on 3 September 2026** and kept only for BRD requirement tracing. They describe the Android/React/REST design that was abandoned. Do not read them as a description of this system |
| `docs/02-data-model.md` | Replaced by the **generated** data dictionary |
| `docs/03-api-specification.md` | Replaced by `04-api-reference.md`. The old file describes a dashboard, an emergency list, search, history and masters endpoints — **none of which exist** |
| `docs/security-test-checklist.md` | Still current, and now one line item inside `07-security-review-checklist.md` |
| `docs/deployment.md`, `azure-deployment.md`, `entra-id-setup.md`, `android-api.md` | Still current. `05-infrastructure.md` is the inventory that points at them |

## Generated, not written

Policy §9: *"Hand-written documentation drifts from reality within months, which is why it
is rarely trusted during a handover."*

| Document | Generator | Run it |
|---|---|---|
| `03-data-dictionary.md` | `docs/tools/generate_data_dictionary.py` | after any change to `src/DI.Vms.Blazor/Data/` |

It reads the EF model — `Entities.cs`, `VmsDbContext.cs`, `FieldLengths.cs` — and fails
loudly rather than writing a half-right document: a class with no `ToTable`, a
`HasMaxLength` naming a property that does not exist, or an unknown CLR type each stop it
with the reason. A data dictionary that is quietly incomplete is worse than one that is
missing, because somebody trusts it.

The same principle already governs `db/004_seed_people.sql`, generated from the AD export
by `db/tools/generate_seed_people.py`.

```
python3 docs/tools/generate_data_dictionary.py
```

## Before go-live

In the order they block each other:

1. **Rotate the four development credentials** — R-02, K-19, gate T3. Not a code change,
   and the highest-value item on the list.
2. **Name the people** — `11-it-asset-registration.md`, `18-sla-and-support.md`,
   `19-incident-response.md`.
3. **Decide the three numbers** — retention (`08-…` §5), RPO and RTO (`17-…` §1).
4. **Execute the testing** — 34 UAT scenarios, 20 security tests, OWASP ZAP baseline.
5. **Test a restore.**
6. **Get the seven deviations approved in writing** — `14-go-live-approval.md` §4.
7. **Sign** `13-uat-signoff.md`, then `14-go-live-approval.md`.

## Open against the policy

Carried here so the gap is one list rather than a search:

| Policy | Gap | Tracked as |
|---|---|---|
| §4.4 audit trail table | None. There is no update or delete path through the application; the real gap is *reads* | D2, R-04 |
| §4.4 FluentValidation | Validation is server-side and unified, but not this library | D3, K-23 |
| §4.4, §7 Azure DevOps | GitHub with equivalent controls | D1, K-24 |
| §7 Semgrep SAST | Not wired | D6, R-07 |
| §6 retention | No period set; nothing deletes a visit | R-05 |
| §4.3 information disclosure | Emirates ID numbers shown and exported in full | R-03 |
| §4.3 audit trail assessment | No record of who read or exported the report | R-04 |

---

*Generated from the codebase per DI-IT-POL-AIDEV-001 §4.6, on 2 October 2026, at commit
`5788a45`.*
