# Entra ID sign-in — setup

What has to exist in the directory before the app can sign anyone in, and why each piece
is there. The application side is done; this is the half that happens in Entra.

Nothing here needs a code change. Everything the app reads is in
`appsettings.Production.json`, which is gitignored and lives only on the server.

---

## 1. Register the application

Entra admin centre → **App registrations** → **New registration**.

| | |
|---|---|
| Name | `DI Visitor Management` |
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
  "TenantId": "…",
  "ClientId": "…",
  "ClientSecret": "…",
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
}
```

That file holds a credential. It should be readable by the app pool identity and
administrators, and nobody else.

## 6. IIS must let the request through

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
