# Entra ID sign-in — setup

What has to exist in the directory before the app can sign anyone in, and why each piece
is there. The application side is done; this is the half that happens in Entra.

Nothing here needs a code change. Everything the server reads is in
`appsettings.Production.json`, which is gitignored and lives only on the server.

## Sign-in is currently OFF

```jsonc
"Authentication": { "Enabled": false }
```

The Entra wiring is in the build and works. What is not finished is the directory work on
this page — the role assignments — and reception needs the desk working before it is. So
sign-in is switched off, and while it is:

- every page and every API endpoint is open to anyone who can reach the server;
- visits are recorded against `(not signed in)` instead of a person;
- the layout carries a **Sign-in is off** badge on every page, and the startup log carries
  a warning, so the state is not quiet.

**The Android app does not sign in at all**, and this page does not apply to it. A
reception tablet sits on a counter and is handed to nobody, so it is not a person's device;
it identifies itself with an API key and its visits are recorded against `(not signed in)`
permanently, not temporarily. See `android/README.md`.

The two coexist. With this switch on, `/api` accepts **either** an Entra bearer token
**or** `X-Vms-Key`, and the same `CanCheckIn` policy then decides
(`Services/ApiKey.cs`, `ApiKeyAuthenticationHandler`). So turning sign-in on for the web
screens does not take the tablets down — but `Api__Key` must be set, or nothing on a tablet
can reach the server.

Turning it on for the web app, once the steps below are done:

1. Set `"Authentication": { "Enabled": true }` in `appsettings.Production.json` and
   restart the app pool.

Nothing else changes. `Services/SignInOptions.cs` explains what the switch does and why it
defaults to off.

## The registration, as it exists

| | |
|---|---|
| Display name | `TS-AI-Visitor Management System` |
| Application (client) ID | `dd0fec3e-2476-4823-a73b-7706c5f8ce7e` |
| Directory (tenant) ID | `ba42ffd1-f322-49fa-81b7-74dcbd5f52a7` |
| Object ID | `0aaaae8c-0f13-4d71-938d-ba5be1ad3727` |
| Supported account types | My organization only |

Those three IDs are recorded here because they are identifiers, not credentials: they
travel in every browser redirect and in the Android app's own configuration, and having
one written-down copy is what stops a second, wrong copy.

**The client secret is different and is never written down here, in any file in this
repository, or in a chat window.** It lives in exactly one place —
`appsettings.Production.json` on UATWEB01. A secret that has been anywhere else is
already spent: delete it in **Certificates & secrets** and create a new one.

---

## 1. Register the application

Entra admin centre → **App registrations** → **New registration**.

| | |
|---|---|
| Name | `TS-AI-Visitor Management System` |
| Supported account types | **Accounts in this organizational directory only** |
| Redirect URI | **Web** — `https://vms.dipark.com/signin-oidc` |

Then **Authentication** → add a **Front-channel logout URL**:
`https://vms.dipark.com/signout-callback-oidc`

Single tenant, deliberately: a visitor log has no business accepting an identity from
another directory.

Note the **Application (client) ID** and **Directory (tenant) ID**.

### For local development

Add a second redirect URI on the same registration —
`https://localhost:7100/signin-oidc` — and logout URL
`https://localhost:7100/signout-callback-oidc`. One registration, two reply URLs, rather
than a second app to keep in step.

## 2. Give it a credential

**Certificates & secrets** → **New client secret**. Copy the **Value**, not the Secret ID.

A secret expires — 24 months at most — and when it does, everyone is locked out at once,
at the desk, with no warning. Put the expiry date in a calendar the day you create it.

**A certificate is the better answer** and this app supports it: replace `ClientSecret`
with a `ClientCertificates` entry pointing at the store. Worth doing before this is more
than a UAT deployment.

## 3. Declare the app roles

**App roles** → **Create app role**, five times. Value is what the app checks; the
display name is what an administrator sees when assigning.

| Value | Display name | What it allows |
|---|---|---|
| `Vms.Officer` | Security Officer | Check visitors in |
| `Vms.Supervisor` | Security Supervisor | + read the visitor report |
| `Vms.Admin` | Administrator | + entities, hosts, configuration |
| `Vms.SystemAdmin` | System Administrator | + audit administration |
| `Vms.UnmaskedId` | View unmasked ID number | Nothing yet — see below |

Allowed member types: **Users/Groups** for all five.

App roles rather than security groups on purpose. A group arrives in the token as an
object id, which means nothing when read in a log or a policy, and once a user is in
enough groups the token stops carrying them at all and starts sending a pointer instead -
at which point the app has to call Graph to find out who it is talking to. An app role
arrives as its own value and says what it is.

`Vms.UnmaskedId` enforces nothing today: Emirates ID numbers are still stored and shown in
full. It is declared now so the grant exists from the start, and so the masking work has
a role to hang on rather than needing a directory change on the day. It is deliberately
**not** implied by seniority — BRD §22 is explicit that being senior is not the same as
needing to see the number.

## 4. Assign people

**Enterprise applications** → `DI Visitor Management` → **Users and groups** → **Add
user/group**.

Assign **groups**, not individuals — `VMS Reception` gets `Vms.Officer`, `VMS Security
Supervisors` gets `Vms.Supervisor`. The role is the app's vocabulary; who is in the group
is the directory's business, and joiners and leavers then need no change here.

Under **Properties**, set **Assignment required?** to **Yes**. Without it, anyone in the
tenant can sign in and simply arrives with no role — which the app handles, but the
directory should be refusing them, not the application.

## 5. Configure the server

In `C:\Websites\vms\appsettings.Production.json`:

```jsonc
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "dubaiinvestments.com",
  "TenantId": "ba42ffd1-f322-49fa-81b7-74dcbd5f52a7",
  "ClientId": "dd0fec3e-2476-4823-a73b-7706c5f8ce7e",
  "ClientSecret": "<the current secret's Value - not its Secret ID>",
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
}
```

That file holds a credential. It should be readable by the app pool identity and
administrators, and nobody else.

## 6. No app roles to add for the tablet

The Android app authenticates with an API key rather than with Entra, so it needs no
platform registration, no redirect URI and no API scope. `Api/VisitsApi.cs` accepts the key
on `X-Vms-Key` whether or not sign-in is on; `android/README.md` explains why the tablet is
a device credential and not a person's.

The key arrives as an identity with one role, `Vms.Officer`, so a tablet can check a
visitor in and do nothing else — not the report, not an unmasked ID number. Those need a
signed-in person.

Should a tablet ever need a signed-in officer, the app's half of this registration — an
`api://<client id>/Visits.Write` scope and an Android platform with the signing keystore's
hash — is in the git history along with the MSAL code.

## 7. IIS must let the request through

**Anonymous authentication ON, Windows authentication OFF.** With Windows authentication
on, IIS challenges the browser before the request reaches the OpenID Connect handler and
sign-in never happens.

`install-iis.ps1` now sets it that way by default. On a site configured before this
change, re-run it, or set it by hand.

Anonymous here means only that *IIS* lets the request through. The application requires a
signed-in user for every page — the authorisation fallback policy is `RequireAuthenticatedUser`,
so a page added later is protected by default rather than by whoever remembers the
attribute.

---

## What single sign-on actually requires

Reception should see no prompt. That happens when the browser can prove who the user is
without asking, which needs one of:

- **Entra-joined or Hybrid-joined desks** — the machine holds a Primary Refresh Token and
  sign-in is silent. This is the one to aim for.
- **Seamless SSO** (Entra Connect) on domain-joined machines, with
  `https://autologon.microsoftazuread-sso.com` in the browser's Local Intranet zone.

Without either, users get a Microsoft sign-in page — still working, but a prompt at a desk
with someone waiting. Worth settling with whoever runs the directory *before* the rollout,
because it is the difference between reception noticing this change and not.

**The desks need to reach `login.microsoftonline.com`.** Card reading is loopback-only, but
sign-in is not: a desk with no internet route can no longer use the application at all.
Check that before deploying, not after.

## Roles in the app

| Screen | Policy | Roles |
|---|---|---|
| New Visitor | `CanCheckIn` | Officer, Supervisor, Admin, SystemAdmin |
| Visitor Report | `CanViewReport` | Supervisor, Admin, SystemAdmin |

A signed-in user with no role gets a page saying so and naming the roles to ask for,
rather than a blank screen or a redirect loop.

## What this closes, and what it does not

**Closes:** anyone who reached the URL could read every visitor record. Now they must be
in the directory, assigned to the application, and hold a role — and every entry records
who saved it (`vms.VisitorEntry.RecordedBy`, added by `db/007_add_recorded_by.sql`), which
the report shows and the export carries.

**Does not close**, and an ITGC review will ask about all three:

1. **Emirates ID numbers, photographs and dates of birth are stored in plain text.**
   `docs/06-security-privacy-rbac.md` specifies masking, envelope encryption under a Key
   Vault master key, and an HMAC lookup hash under a pepper — and is right that an unkeyed
   hash of a `784`-prefixed number is effectively plaintext.
2. **There is no audit log.** Knowing who recorded an entry is not the same as recording
   who *read* one. §22 wants reads audited, especially of unmasked numbers.
3. **No retention limit.** The screen tells the visitor their data is kept "only for this
   visit". Nothing deletes it.

Sign-in was the first of these and the smallest.

---

## The staff directory (the "Person to visit" list)

The host list at the desk reads Entra ID directly, so it is the same directory people sign
in with: a joiner appears without a new AD export, a leaver stops appearing without one, and
a change of title or company follows the tenant.

It replaces `db/004_seed_people.sql` as the *source*. It does not replace `vms.Person`:
`vms.VisitorEntry.PersonToVisitId` points there, and a visit has to stay readable years
after the person has left the tenant. So the table becomes a record of the people who have
actually been visited — a row is written the first time somebody is picked, matched on the
Entra object ID, and kept afterwards. The rows the export already loaded are adopted on
their email address rather than duplicated. Run `db/010_add_person_directory_object_id.sql`
before deploying the build that does this; the app checks for the column at startup and
refuses to run without it.

### The permission to grant

Graph is called with the application's own identity, not the receptionist's — the desk must
be able to look a host up whether or not anyone has signed in.

1. **App registrations → the VMS registration → API permissions → Add a permission →
   Microsoft Graph → *Application* permissions → `User.Read.All`.**
   It must be the Application column, not Delegated. A delegated permission is exercised on
   behalf of a signed-in user, and there may not be one.
2. **Grant admin consent** for the tenant. Without this every directory read fails with
   `Authorization_RequestDenied`, which the app logs in full and the picker reports as
   "The staff directory could not be read".
3. `User.Read.All` is read-only over user objects. `Directory.Read.All` also works and is
   broader than this needs; prefer the narrower one.

### The settings

In the Web App's **Configuration → Application settings** (never in `appsettings.json`,
which is committed):

| Setting | Value |
| --- | --- |
| `Directory__Source` | `EntraId`, or `Database` to go back to the exported table |
| `Directory__TenantId` | the directory (tenant) ID |
| `Directory__ClientId` | the application (client) ID |
| `Directory__ClientSecret` | the secret **value**, not its ID |

Each falls back to the `AzureAd` equivalent, so a deployment where sign-in and the directory
read use the same registration needs only `Directory__Source`. Left entirely unset, the app
uses Entra ID when it has credentials and the table when it does not.

Optional, and rarely wanted: `Directory__CacheMinutes` (default 20 — see below),
`Directory__MaximumUsers` (default 20000), `Directory__MaximumSuggestions` (default 12).

### How it behaves

The whole directory is fetched once, paged, and searched in memory. A type-ahead issues a
request every few characters; one Graph call per keystroke per desk is a great deal of
traffic against a tenant-wide throttle for a list that changes when somebody joins. The
copy is re-fetched every `Directory__CacheMinutes`.

Two consequences worth knowing:

- Focusing the empty field lists the directory, because the list is already in memory.
  Nothing needs to be typed first.
- A Graph outage costs the desk nothing until the copy expires, and even then the last good
  copy is kept and served rather than the picker going blank — with a note on the field
  saying the list may be out of date. Stale names beat no names at a desk with a visitor
  waiting.

The **entity filter does not apply** to the directory. "Entity being visited" is which group
company is being visited; the directory's `companyName` is the tenant's spelling, which is
not the entity list's — mapping one to the other is guesswork, and a wrong guess hides a
host rather than narrowing to them. The checkbox is therefore hidden when the directory is
the source. The visit still records both, independently.

Disabled accounts are filtered out (`accountEnabled eq true`). Guests and service accounts
are not: a guest can be somebody's host, and a rule that guesses which accounts are people
belongs in the tenant, not in a visitor book.
