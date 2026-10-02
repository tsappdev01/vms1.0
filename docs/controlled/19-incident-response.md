# Incident Response Procedure

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Incident Response Procedure (Controlled, required · IT)

For a **security or data incident**. An ordinary fault goes to support
(`18-sla-and-support.md`). If you are not sure which this is, **treat it as an incident** —
the cost of being wrong that way is a meeting.

---

## 1 · Contacts

Fill these in before go-live. A procedure with blank contacts is a procedure nobody can
follow at 7 a.m. on a Sunday.

| Role | Name | Primary | Out of hours |
|---|---|---|---|
| Incident lead (IT) | `[ ]` | `[ ]` | `[ ]` |
| IT Head | `[ ]` | `[ ]` | `[ ]` |
| InfoSec | `[ ]` | `[ ]` | `[ ]` |
| Business Owner | `[ ]` | `[ ]` | `[ ]` |
| Data protection / legal | `[ ]` | `[ ]` | `[ ]` |
| ICP contact | `[ ]` | `[ ]` | — |
| Azure / infrastructure | `[ ]` | `[ ]` | `[ ]` |

## 2 · What counts

Anything on this list is an incident. Report it even if you think it was nothing.

| | Example |
|---|---|
| **Credential exposure** | A key or password in a file, a message, a screenshot, a ticket |
| **Device loss** | A reception tablet lost or stolen |
| **Unauthorised access** | Somebody reaching the report who should not; an unexplained sign-in |
| **Data disclosure** | A CSV export sent outside the group; visitor details emailed |
| **Unexplained change** | A visit in the database that nobody recorded, or one that has changed |
| **Compromise of the host** | Any sign of it, on UATWEB01, the Web App, or a reception PC |
| **Supply chain** | A vulnerability in the ICP toolkit or a dependency being actively exploited |

### Two things that are *not* incidents

- **A card that will not read**, including toolkit error 233. It is a fault with a
  documented fallback. S2 in support.
- **An expired visitor card.** The system warns and records; it is not a breach.

## 3 · What to do, in order

### Step 1 — Contain, within the first minutes

| Incident | Do first |
|---|---|
| Tablet lost or stolen | **Rotate that tablet's API key** (`16-admin-guide.md` §5). Leave the other two alone — that is what one key per tablet is for |
| Credential exposed | **Rotate it.** Do not wait to find out whether it was used. For a SQL password, rotation is the only remedy — the old value cannot be unseen |
| Unauthorised access via a role | Remove the Entra app-role assignment. The change takes effect at their next sign-in; end the session if you need it sooner |
| Host compromise suspected | Take the host out of service. The database is a separate asset; do not restore over it yet |
| Export sent outside | Nothing technical to contain. Go straight to Step 3 — this is a disclosure |

**Do not delete anything, and do not "tidy up".** Logs and the current state are the only
evidence there is.

### Step 2 — Tell people

Within `[ ]` of discovery: the incident lead, the IT Head, and InfoSec. The Business Owner
as soon as visitor data may be involved.

Say what you know and what you do not. A first report that says "an export may have left
the group, I do not yet know to whom" is more useful than a complete one an hour later.

### Step 3 — Assess

| Question | Where to look |
|---|---|
| What data could be involved? | `08-data-classification.md` — this is why it exists |
| How many people? | The report, over the affected period |
| Does it include Emirates ID numbers? | **Assume yes.** Numbers are stored and displayed in full (R-03) |
| Does it include photographs or card images? | `vms.VisitorCardImage` and `VisitorEntry.Photo` |
| Who accessed the report, and when? | **You cannot answer this. There is no access log** (R-04) |
| Was anything changed in the database? | **You cannot answer this from the application.** There is no edit path through it, so any change was made directly at the database. SQL Server auditing on the instance is the only source |

> **Two of those questions have no answer today, and both are known gaps.** Say so in the
> report rather than guessing. If an incident ever turns on either of them, that is the
> argument for closing R-03 and R-04.

### Step 4 — Eradicate and recover

1. Fix the cause, not the symptom.
2. Rotate anything that could have been seen, not only what you know was.
3. Restore from backup only if data was lost or altered — `17-backup-and-recovery.md`.
4. Verify: `GET /health`, a test check-in, and the report over the affected period.
5. Confirm with reception that the system is behaving.

### Step 5 — Record

Every incident gets a record, including ones that turned out to be nothing.

| Field | |
|---|---|
| Reference | `[ ]` |
| Discovered | date, time, by whom, how |
| What happened | |
| Data involved | class, number of people, whether Emirates ID numbers |
| Contained at | time, and what was done |
| Root cause | |
| Fixed by | |
| Notified | who, when |
| Follow-up actions | with owners and dates |

### Step 6 — Review

Within `[ ]` working days, with the incident lead, IT Head and Business Owner:

- Did the procedure work? If a step was skipped, why?
- Does a risk in `10-risk-assessment.md` need re-rating, or a new row?
- Does a control need to change?
- **Was there a question we could not answer?** That is usually the most useful output.

## 4 · Regulatory notification

> **[DECISION REQUIRED — legal / data protection]** Whether and within what period a
> disclosure of visitor personal data must be notified to a UAE authority or to the
> individuals, and who makes that call.
>
> This is not IT's to decide and is left blank deliberately. What IT provides is the
> assessment in Step 3; the notification decision is the business's.

## 5 · The standing exposure

Recorded here because an incident involving any of it would start from a known position
rather than a surprise:

Four credentials were pasted into a development chat session — the Entra client secret, the
SQL password, the `vmsdi` storage AccountKey, and an API key. **None has yet been rotated.**
The SQL password is additionally in this repository's git history permanently.

If any of these is implicated in an incident, the containment step is already written: it
is R-02, and it is the first row of the technical gate on `14-go-live-approval.md`.

## 6 · Test

| | |
|---|---|
| Frequency | Annually, and before go-live |
| Method | Tabletop. Suggested scenario: *a reception tablet is missing at the end of a shift and nobody knows for how long* — it exercises containment, the one-key-per-tablet design, the "no visitor data on the device" fact, and the access-log gap |
| Evidence | Notes on the review work item |

| Date | Scenario | Run by | Findings |
|---|---|---|---|
| | | | |
