# Risk Assessment

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Risk Assessment (Controlled, required)

| | |
|---|---|
| Assessed by | IT, from the codebase |
| Business owner | *(to be named — `11-it-asset-registration.md`)* |
| Date | 2026-10-02 |
| Next review | Quarterly, per policy §2 |

Scoring: **Likelihood** and **Impact** each Low / Medium / High. A risk is **Open** until
the stated treatment is done. Nothing here is softened; a risk register that reads well is
not doing its job.

---

## Summary

| ID | Risk | L | I | Rating | Status |
|---|---|:-:|:-:|:-:|---|
| R-01 | Tablet card reads cannot be proven genuine | M | H | **High** | Open — with ICP |
| R-02 | Four development credentials not yet rotated; one SQL password permanently in git history | H | H | **High** | Open — owner actioning |
| R-03 | Emirates ID numbers stored, displayed and exported in full | H | M | **High** | Open |
| R-04 | No record of who read, exported or printed the report | H | M | **High** | Open |
| R-05 | No retention period; nothing ever deletes a visit | H | M | **High** | Open — decision needed |
| R-06 | ICP Service Provider licence expires; card reading stops when it does | M | H | **High** | Open — calendar control |
| R-07 | No SAST (Semgrep) step in CI, required by policy §7 | M | M | **Medium** | Open |
| R-08 | One tablet returns toolkit error 233 while another on the same network works | H | M | **Medium** | Open — unresolved |
| R-09 | Go-live in `Authentication:Enabled = false` would bypass all access control | L | H | **Medium** | Open — gate at go-live |
| R-10 | Database credentials grant unaudited full access to all visitor data | M | H | **High** | Open — accepted with controls |
| R-11 | A visit left open indefinitely makes the evacuation list wrong | M | M | **Medium** | Open — accepted, deliberate |
| R-12 | Deployment depends on hardware and a toolkit with a single supplier | M | M | **Medium** | Accepted |
| R-13 | Card image CSV/print output leaves the system entirely | M | M | **Medium** | Open |
| R-14 | Key-person dependency on the AI-assisted build | L | M | **Low** | Mitigated |

---

## R-01 · Tablet card reads cannot be proven genuine

**Likelihood M · Impact H · High**

A desk read produces signed XML which the server verifies. The tablet's offline bundle
returns **unsigned** responses, so `/api/visits` stores what the tablet sends without being
able to prove a chip produced it. Anyone holding a tablet's API key could post a fabricated
visitor.

Mitigated in part: one key per tablet, keys behind a PIN, rate limiting on the public host,
and the server re-applies every field rule including the Luhn check digit.

**Treatment** — Open with ICP; see `../icp-signed-response-request.md`. Until ICP supplies a
signing path for the offline bundle, tablet check-ins are a lower assurance than desk
check-ins and `CaptureMethod` lets the report tell them apart. **Do not** let a tablet key
reach a device outside IT's control.

## R-02 · Development credentials not yet rotated

**Likelihood H · Impact H · High**

Four credentials were pasted into a development chat session: the Entra client secret, the
SQL password, the `vmsdi` storage AccountKey, and an API key. The SQL password is also in
this repository's git history, where it cannot be removed.

**Treatment** — Rotate all four. For the SQL password, rotation is the only remedy;
rewriting history does not help once the value has been read. Owner has confirmed this will
be done. **This is the single highest-value item on this register and it is not a code
change.**

## R-03 · Emirates ID numbers in full

**Likelihood H · Impact M · High**

Stored unmasked, shown unmasked on the New Visitor confirmation, in the report, in the
sign-out list and in the CSV export. `Vms.UnmaskedId` exists as a role; nothing calls the
`CanViewUnmaskedId` policy.

**Treatment** — Mask to `784-****-*****69-1` by default in every display and in the CSV;
show in full only to `CanViewUnmaskedId`, and log each unmasking. The role already exists in
the tenant, so this needs no directory change on the day.

## R-04 · No record of report reads and exports

**Likelihood H · Impact M · High**

The report shows every visitor's full identity. A CSV export leaves the system and goes to
whatever machine asked for it. **Nothing records that either happened.** The one place
personal data leaves in bulk is the one place with no trail.

**Treatment** — An access log: who opened the report, over what date range, what they
exported, and who opened a card image. This was recommended **in place of** a general audit
table, because visits cannot be edited or deleted through the application so the row is
already its own record; the gap is reads, not writes.

## R-05 · No retention period

**Likelihood H · Impact M · High**

Nothing deletes a visit. Rows, card images and chip photographs accumulate indefinitely.

**Treatment** — Business decision required: how long a visit is kept, whether the card image
and photograph are kept for a shorter period than the text, and who approves a deletion
run. Then a scheduled job and a script in `db/`. See `08-data-classification.md` §5.

## R-06 · ICP licence expiry

**Likelihood M · Impact H · High**

Every device is registered with the ICP Validation Gateway against a Service Provider
licence, and **the licence expires**. When it does, card reading stops at every desk at
once, with visitors standing there.

**Treatment** — A calendar reminder 60 days before expiry, owned by IT, not by whoever
remembers. The reader panel shows `licence to <date>`; record that date in
`11-it-asset-registration.md` and review it quarterly. Manual entry is the documented
fallback and works without the licence.

## R-07 · No SAST in CI

**Likelihood M · Impact M · Medium**

Policy §7 requires every merge to run a NuGet audit, a **Semgrep SAST scan** and build
verification. The first and third are in `.github/workflows/dotnet.yml`. Semgrep is not.

**Treatment** — Add a Semgrep step. Report-only to begin with, for the same reason the
dependency audit is report-only.

## R-08 · Toolkit error 233 on one tablet

**Likelihood H · Impact M · Medium**

One tablet returns toolkit error 233 (`ETSTATUS_SERVER_RESPONSE_ERROR`) on every read while
another tablet on the same network reads normally.

Root cause **established**: the toolkit reaches ICP's Validation Gateway during
`readPublicData`, and the failing tablet cannot. It is not a build regression — the
previous APK fails on the same tablet.

**Why that tablet and not the other is unresolved.** Two things are outstanding and both
are information, not code: the `Toolkit:Agent:ToolkitConfig` value the server uses (the
desk reads offline successfully on the same licence), and a comparison of the reader-panel
line — `<reader name> · Toolkit <version> · licence to <date>` — between the two tablets.

**Treatment** — `read_publicdata_offline = true` is set, and a Settings switch ("Read cards
without ICP's gateway", default off) withholds `config_ag`. Collect the two facts above and
raise with ICP if they do not explain it.

> Recorded for the handover, because it is the kind of thing that is expensive to learn
> twice: this flag was once removed on the strength of a search of the wrong binary
> (`libc++_shared.so`, the C++ runtime, and `classes.jar`, which is compressed so `strings`
> finds nothing in it). The binary that actually ships it is
> `libEIDAToolkitJNIWrapper.so`. **Search the binary that ships.**

## R-09 · Go-live with sign-in off

**Likelihood L · Impact H · Medium**

`Authentication:Enabled = false` is a supported configuration for a desk before the tenant
is wired. In that mode every screen is reachable by anyone who can reach the host.

**Treatment** — A go-live gate: `14-go-live-approval.md` requires the value to be confirmed
`true` in the production configuration before approval is signed.

## R-10 · Database credentials bypass everything

**Likelihood M · Impact H · High**

The application's access control is real; the database's is a separate matter. Anyone with
the SQL login can read every visitor's identity, and nothing in VMS would show it.

**Treatment** — Accepted with controls: the login is granted only what it needs
(`db/006_grant_app_login.sql`, `db/008_grant_app_user_azure.sql`), the credential lives in
app settings and never in a committed file, and SQL Server auditing is IT's standard
control on the instance rather than this application's to implement.

## R-11 · Visits left open

**Likelihood M · Impact M · Medium**

A visitor who leaves without telling reception stays signed in until somebody signs them
out. The "still in the building" list — which is what an evacuation would be run from —
over-reports.

**Treatment** — Accepted, and **deliberate**. Nothing closes a visit automatically, because
a system that closed entries at midnight would produce an evacuation list that was tidy and
wrong. The sign-out screen offers "sign out everyone" for the end of the day. If a
scheduled close is ever wanted, it must be visibly distinguishable from a real sign-out.

## R-12 · Single-supplier hardware and toolkit

**Likelihood M · Impact M · Medium**

Card reading depends on ICP's toolkit v3.1.6, a PC/SC reader at each desk, and an ACS
reader on each tablet. On mobile the reader must match a shipped plugin.

**Treatment** — Accepted. Manual entry works without any of it and is a first-class path,
not a degraded one: it has the same field rules and the same server-side validation.

## R-13 · Report output leaves the system

**Likelihood M · Impact M · Medium**

The CSV export and the print view carry full visitor identity onto a supervisor's own
machine and out of every control this application has.

**Treatment** — Partly answered by R-03 (mask the number in the export) and R-04 (log that
the export happened). The residue — a file on a laptop — is an endpoint control, not an
application one.

## R-14 · Key-person dependency

**Likelihood L · Impact M · Low**

Policy §9 is explicit that this is the risk an AI-assisted build is meant to reduce, and
asks for four controls rather than documentation alone.

**Mitigated:**

- Standard stack throughout — C#/.NET, Blazor, SQL Server, Entra ID
- XML documentation comments on public members; comments explain *why*, not *what*
- `src/DI.Vms.Blazor/README.md` records what was learned from the SDK and from real cards,
  and why each non-obvious decision was made
- The data dictionary is **generated** from the model (`docs/tools/`), so it cannot drift
- Every SQL script is in `db/`, numbered, re-runnable, and the seed script is generated from
  its source rather than hand-written

**Residual:** the Azure DevOps controls in policy §7 are met on GitHub instead. See §11 of
the compliance index — this is a deviation requiring written Policy Owner approval.
