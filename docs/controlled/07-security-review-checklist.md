# Security Review Checklist

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Security Review Checklist (Controlled, required)
Covers the policy's §4.3 AI code analysis categories and §4.5 Scenario 3 (Security and
Access Testing, IT-owned).

`../security-test-checklist.md` is the detailed XSS and SQL-injection source audit and the
manual test to run at a desk. This document is the whole checklist, with that one as a
line item.

**Legend** — ✅ met · ⚠️ partly met · ❌ not met · ⬜ to be executed by IT

---

## A · Stage 4 mandatory controls (policy §4.4)

| # | Control | State | Evidence |
|---|---|:-:|---|
| A1 | **Entra ID SSO** — users authenticate with existing M365 credentials, no separate login | ✅ | `Program.cs`, `../entra-id-setup.md`. Configurable off for a desk before the tenant is wired; **must be on at go-live** (R-09) |
| A2 | **Role-based access control enforced server-side on every API route** | ✅ | `Services/VmsRoles.cs`, `Program.cs`. `FallbackPolicy = DefaultPolicy` — nothing is anonymous. Matrix: `09-access-control-matrix.md` |
| A3 | **Audit trail table in MSSQL** — every create, update and delete with UserID, timestamp, action, old and new values | ❌ | **Deviation, approved by the owner, requiring written Policy Owner sign-off.** There is no update or delete path through the application, so the row is its own record; `RecordedBy` and `RecordedAtUtc` capture the create. The real gap the policy is reaching for here is *reads* — see R-04 |
| A4 | **Input validation using FluentValidation on all server-side entry points** | ✅ | `Services/VisitValidators.cs` — `SaveVisitRequestValidator` (rule sets Request / TypedIdentity / Contact) and `CardIdentityValidator`, resolved from DI into `POST /api/visits` in both hosts. A **wrapper** over `Services/VisitorFields.cs`, not a second opinion: the judgements stay in the one place the screens and the tablet are written against |
| A5 | **Security headers middleware** — CSP, X-Frame-Options, X-Content-Type-Options, HSTS | ✅ | `Services/SecurityHeaders.cs`. Also Referrer-Policy `same-origin` and Permissions-Policy `camera=(self)` |
| A6 | **MSSQL database**, all data in a governed database | ✅ | Database `VMS`, schema `vms`. No Excel, no local storage, no shared drive |
| A7 | **Azure DevOps repository** with branch policies and PR review | ❌ | **Deviation.** The repository is GitHub (`tsappdev01/vms1.0`) with equivalent controls. Requires written Policy Owner approval |
| A8 | **IIS hosting on Windows Server**, no additional runtime dependencies | ⚠️ | UATWEB01 is IIS on Windows. The Azure deployment is a Windows App Service plan. The ICP toolkit and the desk agent are additional dependencies **by necessity** — a chip cannot be read without them |
| A9 | **XML documentation comments on every public method**; inline comments for non-obvious business rules only | ✅ | Throughout. `src/DI.Vms.Blazor/README.md` carries the longer arguments |

## B · Stage 3 code analysis categories (policy §4.3)

| # | Category | Finding |
|---|---|---|
| B1 | **Data audit** — what is collected, whether PII is present, where it is transmitted | Complete. `08-data-classification.md` and `02-data-flow.md`. **PII is present and is the point of the application.** One outbound flow only: ICP's Validation Gateway |
| B2 | **Authentication gaps** | None found in the application. `FallbackPolicy` closes the "forgot the attribute" class of gap entirely. The gap is operational: sign-in can be configured off (R-09) |
| B3 | **Injection risks** | Audited. EF Core parameterises everything; no raw SQL is built from input. A typed `%` in the host search is escaped so it cannot match the whole directory. Detail and test: `../security-test-checklist.md` |
| B4 | **Input validation failures** | Addressed. One definition of the rules (`VisitorFields`) used by the desk, the tablet and the API, because three copies disagreed before it existed — the web required a mobile number and the tablet called it optional, and the server asked only whether the ID number contained a digit, so `7` was a valid Emirates ID |
| B5 | **Information disclosure** | **Two findings.** Emirates ID numbers are displayed and exported in full (R-03). The staff directory is correctly *not* disclosed — `/api/people` searches server-side and returns at most 12 matches |
| B6 | **Insecure storage** | Credentials are correctly excluded from committed files and from the APK. **But four development credentials are not yet rotated and one SQL password is permanently in git history** (R-02) |
| B7 | **Audit trail assessment** — does the application log who did what and when | **Writes: yes** (`RecordedBy`, `RecordedAtUtc`, `SignedOutBy`, `SignedOutAtUtc`). **Reads: no** (R-04) |
| B8 | **Business logic risk** — calculation errors, edge cases, single points of failure | The one arithmetic in the system is the Emirates ID Luhn check digit and the TD1 zone check digits, both verified against every card this project has seen. Single points of failure: the ICP licence (R-06) and the reader hardware (R-12) |
| B9 | **Migration complexity** | Not applicable. Built on the standard stack; there is nothing to migrate from |

## C · Application security controls

| # | Control | State | Where |
|---|---|:-:|---|
| C1 | HTTPS everywhere, HSTS | ✅ | `Services/SecurityHeaders.cs` |
| C2 | Content-Security-Policy | ✅ | Composed from configuration so the OCR CDN and the ICP agent WebSocket are allowed deliberately rather than by a blanket `unsafe-*` |
| C3 | Cookies HttpOnly, Secure always, SameSite=Strict | ✅ | `Program.cs` |
| C4 | Rate limiting on the public API | ✅ | 300/min per caller IP; rejections logged with address and path. Not registered on UATWEB01, which is office-network only |
| C5 | API key per tablet, not one shared key | ✅ | Three keys; a visit records which desk saved it |
| C6 | No secret in the Android APK | ✅ | Public client. `VMS_API_KEY` comes from `%USERPROFILE%\.gradle\gradle.properties` |
| C7 | Tablet settings behind a PIN | ✅ | PBKDF2-HMAC-SHA256, constant-time compare, wall-clock lockout with a cap, no recovery path |
| C8 | Card read signature verified server-side | ⚠️ | Desk reads: yes. **Tablet reads are unsigned** (R-01) |
| C9 | Request ID spent once, issued by the server | ✅ | `POST /api/reads` — the device cannot choose it and cannot prepare a response before being asked |
| C10 | Card image served with its own hard CSP and nosniff | ✅ | `GET /visits/{id}/card` |
| C11 | Field length clamping before the database | ✅ | `FieldLengths.Clamp` — so a response the server could not verify becomes a refusal, not a truncation error at a desk |
| C11b | Save rules declared, not scattered | ✅ | Every refusal `POST /api/visits` can return is declared in `Services/VisitValidators.cs` and can be read without reading the endpoint |
| C12 | Emirates ID masking | ❌ | R-03 |
| C13 | Read/export access log | ❌ | R-04 |
| C14 | Retention and deletion | ❌ | R-05 |
| C15 | Secrets excluded from the repository | ✅ | `.gitignore`, deployment templates, and a CI step that greps the tracked tree |
| C16 | Credentials rotated after development | ❌ | R-02 |

## D · Pipeline controls (policy §7)

| # | Control | State |
|---|---|:-:|
| D1 | All changes via pull request; direct commits to main blocked | ⬜ IT to confirm the branch protection rule |
| D2 | Minimum one IT reviewer approves before merge | ⬜ IT to confirm |
| D3 | NuGet audit on every merge | ✅ `dotnet list package --vulnerable --include-transitive` |
| D4 | Deprecated package report | ✅ |
| D5 | **Semgrep SAST on every merge** | ❌ R-07 |
| D6 | Build verification on every merge | ✅ Both projects |
| D7 | Secrets never committed; values injected at deployment | ✅ plus a grep backstop in CI |
| D8 | Tamper-evident version history | ✅ Git |

## E · Scenario 3 — Security and Access Testing (IT executes)

Fully IT-owned per §4.5. To be run on staging before go-live and recorded here.

| # | Test | Expected | Result | By | Date |
|---|---|---|---|---|---|
| E1 | Call `GET /api/reference` with no credential | 401 | ⬜ | | |
| E2 | Call `GET /api/reference` with a wrong `X-Api-Key` | 401 | ⬜ | | |
| E3 | Call `POST /api/visits` with a valid key but a missing mobile number | 400, title `The visit was not saved` | ⬜ | | |
| E4 | Call `POST /api/visits` with a spent `requestId` | 400, title `The card read was not accepted` | ⬜ | | |
| E5 | Sign in as **Officer**, open `/report` | Access denied | ⬜ | | |
| E6 | Sign in as **Officer**, request `GET /visits/1/card` directly | Access denied | ⬜ | | |
| E7 | Sign in as **Supervisor**, open `/report` | Permitted | ⬜ | | |
| E8 | Sign in as a user with **no VMS role**, open `/` | Access denied | ⬜ | | |
| E9 | Open any page signed out | Redirected to sign-in | ⬜ | | |
| E10 | POST a typed ID number failing the Luhn check (`784197940769281`) | 400, refused | ⬜ | | |
| E11 | Submit `'; DROP TABLE vms.VisitorEntry--` as a host name | Stored as text; table intact | ⬜ | | |
| E12 | Submit `<script>alert(1)</script>` as a host name, then open the report | Rendered as text, no script runs | ⬜ | | |
| E13 | Submit `%` as the host search term | Does not return the whole directory | ⬜ | | |
| E14 | Exceed 300 requests/minute against `/api` on the public host | 429, rejection logged with address and path | ⬜ | | |
| E15 | Inspect response headers on any page | CSP, HSTS, X-Frame-Options DENY, nosniff, Referrer-Policy, Permissions-Policy all present | ⬜ | | |
| E16 | Inspect the session cookie | HttpOnly, Secure, SameSite=Strict | ⬜ | | |
| E17 | Decompile the shipped APK and search for the API key and any client secret | Neither present | ⬜ | | |
| E18 | Enter a wrong tablet PIN repeatedly | Lockout with a visible countdown; no bypass | ⬜ | | |
| E19 | **OWASP ZAP baseline scan against staging** | No High findings; Medium findings triaged and recorded | ⬜ | | |
| E20 | Confirm `Authentication:Enabled = true` in the production configuration | Confirmed | ⬜ | | |

E11, E12 and E13 have already been audited at source in `../security-test-checklist.md`;
they are repeated here because Scenario 3 is a *test*, not a code review.

---

## Sign-off

This checklist is not complete until every ⬜ in section E carries a result, and every ❌ is
either closed or carries written Policy Owner approval as a deviation.

| | Name | Signature | Date |
|---|---|---|---|
| Reviewed by (IT / InfoSec) | | | |
| Deviations approved by (Policy Owner) | | | |
