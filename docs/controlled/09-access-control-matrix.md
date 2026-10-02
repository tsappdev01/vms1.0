# Access Control Matrix

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Access Control Matrix (Controlled, required)

Generated from `Services/VmsRoles.cs`, `Program.cs`, the `[Authorize]` attributes on
`Components/Pages/`, and `Api/VisitsApi.cs`. `../entra-id-setup.md` is how the roles are
created and assigned in the tenant.

---

## 1. Roles

Entra ID **app roles**, not security groups. A group membership arrives as an object id
that means nothing when read in a log or a policy, and the token stops carrying groups once
a user is in enough of them. App roles arrive in the `roles` claim as their own value and
say what they are.

| Role | Value | Who |
|---|---|---|
| Officer | `Vms.Officer` | Works a reception desk |
| Supervisor | `Vms.Supervisor` | Works a desk and reads the report |
| Admin | `Vms.Admin` | IT, day to day |
| System Admin | `Vms.SystemAdmin` | IT, full |
| Unmasked ID | `Vms.UnmaskedId` | Granted per person to see an Emirates ID number in full |

`Vms.UnmaskedId` is **deliberately not implied by any of the others**. Seniority is not the
same thing as a need to see the number. It is granted individually.

## 2. Policies

| Policy | Satisfied by |
|---|---|
| `CanCheckIn` | Officer, Supervisor, Admin, SystemAdmin |
| `CanViewReport` | Supervisor, Admin, SystemAdmin |
| `CanViewUnmaskedId` | UnmaskedId **only** |

`options.FallbackPolicy = options.DefaultPolicy`. **Nothing is anonymous.** A page added
later is protected by default rather than by whoever remembers the attribute — the right
way round for a visitor log.

## 3. The matrix

✔ permitted · ✘ refused

| Capability | Where enforced | Officer | Supervisor | Admin | SystemAdmin | UnmaskedId | Tablet (API key) |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|
| Check a visitor in | `NewVisitor.razor`, `POST /api/visits` | ✔ | ✔ | ✔ | ✔ | ✘ | ✔ |
| Read a card / start a read | `POST /api/reads` | ✔ | ✔ | ✔ | ✔ | ✘ | ✔ |
| Preview a photographed card | `POST /api/mrz` | ✔ | ✔ | ✔ | ✔ | ✘ | ✔ |
| List entities and purposes | `GET /api/reference` | ✔ | ✔ | ✔ | ✔ | ✘ | ✔ |
| Search for a host | `GET /api/people` | ✔ | ✔ | ✔ | ✔ | ✘ | ✔ |
| Sign a visitor out | `SignOut.razor` | ✔ | ✔ | ✔ | ✔ | ✘ | ✘ |
| Read the visitor report | `VisitorReport.razor` | ✘ | ✔ | ✔ | ✔ | ✘ | ✘ |
| Export the report to CSV | `VisitorReport.razor` | ✘ | ✔ | ✔ | ✔ | ✘ | ✘ |
| Print the report | `VisitorReport.razor` | ✘ | ✔ | ✔ | ✔ | ✘ | ✘ |
| Open a stored card image | `GET /visits/{id}/card` | ✘ | ✔ | ✔ | ✔ | ✘ | ✘ |
| See an Emirates ID number in full | **nothing enforces this yet** | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Edit a saved visit | **no such capability exists** | ✘ | ✘ | ✘ | ✘ | ✘ | ✘ |
| Delete a saved visit | **no such capability exists** | ✘ | ✘ | ✘ | ✘ | ✘ | ✘ |
| Change the entity list | SQL script in `db/`, run by IT | ✘ | ✘ | ✔ | ✔ | ✘ | ✘ |
| Change the tablet's server address or key | tablet Settings, **behind a PIN** | — | — | — | — | — | — |

The tablet column reflects what the API key reaches, not a role: the key is a way of
arriving and `CanCheckIn` is what decides. A tablet cannot read the report because the
report is not an API endpoint.

## 4. The two rows that are not yet true

**"See an Emirates ID number in full" is ✔ for everyone.** The role exists, the policy
exists, and nothing calls it. Numbers are stored and displayed in full on the New Visitor
confirmation, in the report, in the CSV, and in the sign-out list. Risk R-03.

**"Edit" and "Delete" are ✘ because the capability does not exist**, not because a role
refuses it. There is no update path and no delete path to a `vms.VisitorEntry` through the
application — the only writes are the insert at check-in and the sign-out stamp. This is
why there is no audit table: a row that cannot be changed is its own record.

That argument holds exactly as far as the application. Anyone with database credentials can
change anything, and nothing in VMS would show it.

## 5. Non-interactive access

| Identity | Reaches | Credential | Held in |
|---|---|---|---|
| Tablet 1 / 2 / 3 | `/api/*` | `X-Api-Key`, **one key per tablet** so a visit records which desk saved it | Tablet settings, behind a PIN; server side in Azure app settings |
| Application → SQL | database `VMS` / `vms` | SQL login granted by `db/006_grant_app_login.sql` (on premises) or `db/008_grant_app_user_azure.sql` (Azure) | Connection string in app settings or `appsettings.Production.json` |
| Application → Blob | card image container | Account key | Azure app settings |
| Desk browser → ICP agent | `wss://localhost` | none — local service | — |

### The tablet settings PIN

Not an Entra role. It guards the one screen on the tablet that is not meant for whoever is
holding it, because that screen holds the API key and will show it on request.

| | |
|---|---|
| Hash | PBKDF2-HMAC-SHA256 |
| Comparison | `MessageDigest.isEqual` — constant time |
| Lockout | Wall-clock, with a cap, counted down on screen |
| Recovery | **None.** Clear the app's data in Android settings — which also clears the server address and the key — and set the tablet up again |

## 6. Sign-in off

`Authentication:Enabled = false` is a supported configuration and is what a reception PC
runs before the tenant is wired. In that mode every screen is reachable, and `RecordedBy`
records **"(not signed in)"** rather than a name.

That string is the point: `RecordedBy` is also `NULL` on rows written before sign-in
existed, and `NULL` means "recorded when the system had no idea who anyone was" rather than
"unknown user". The two are kept distinguishable instead of one being backfilled into
something that reads like a name.

**A Controlled application may not go live in this mode.** Policy §3 requires Entra ID SSO.

## 7. Review

| | |
|---|---|
| Role assignments reviewed | Quarterly, per policy §2 for Controlled applications |
| Reviewed by | IT, against the current reception roster |
| Evidence | Entra ID app-role assignment export, attached to the review work item |
| Last review | *(none — application has not gone live)* |
