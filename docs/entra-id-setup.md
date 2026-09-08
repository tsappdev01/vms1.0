# Entra ID sign-in — setup

What has to exist in the directory before the app can sign anyone in, and why each piece
is there. The application side is done; this is the half that happens in Entra.

Nothing here needs a code change. Everything the server reads is in
`appsettings.Production.json`, which is gitignored and lives only on the server.

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

## 6. Two more things, for the Android app

The tablet uses the same registration. It is a public client — an app on a device cannot
keep a secret — so it authenticates by being the app it says it is, and gets a token for
the server's own API rather than reusing the server's cookie.

**Expose an API.** *Expose an API* → the Application ID URI is `api://<client id>`, which
is the default → **Add a scope**:

| | |
|---|---|
| Scope name | `Visits.Write` |
| Who can consent | Admins and users |
| Admin consent display name | Record visitor entries |

This is the scope the tablet asks for and the audience `Api/VisitsApi.cs` checks. A Graph
token will not do, and neither will the cookie a desk browser holds — which is the point:
a session cookie lifted from a desk cannot be replayed against the API.

**Add the Android platform.** *Authentication* → **Add a platform** → **Android**:

| | |
|---|---|
| Package name | `ae.dubaiinvestments.vms` |
| Signature hash | from the signing keystore — `android/README.md` has the command |

That produces the redirect URI `msauth://ae.dubaiinvestments.vms/<hash>`. The hash belongs
to the keystore, not to the source, so debug and release builds have different ones and
both need registering.

No role changes. The API requires the `CanCheckIn` policy, which
`Vms.Officer`, `Vms.Supervisor`, `Vms.Admin` and `Vms.SystemAdmin` already satisfy — so
anyone who can check a visitor in on the web can do it on the tablet, and nobody else can.

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
