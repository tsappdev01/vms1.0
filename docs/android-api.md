# The API the Android tablet uses

Four endpoints. The tablet never touches SQL Server — a connection string on a device that
leaves the building is a credential that has left the building, so the device talks to the
application and the application talks to the database.

Source: `src/DI.Vms.Blazor/Api/VisitsApi.cs` (server) and
`android/app/src/main/java/ae/dubaiinvestments/vms/api/` (client).

## Is it switched on?

**The endpoints are always mapped. Whether a *tablet* can reach them is a separate question,
and the answer is one configuration value.**

```csharp
app.MapVisitsApi(signIn.Enabled, acceptTabletKey: signIn.Enabled && apiKey is not null);
```

Sign-in is on for this deployment, so the tablet's key is accepted **only when `Api:Key` is
set**. Without it the tablet gets `401` on every call, and nothing on the tablet explains why —
the server is simply not listening for that way in.

The application says which state it is in, once, at startup:

```
The /api endpoints are guarded by an Entra ID token or the tablet's API key
The /api endpoints are guarded by an Entra ID token - no Api:Key is set, so no tablet can reach them
The /api endpoints are guarded by an API key
The /api endpoints are guarded by nothing but the network this server is on
```

Read that line in the container log before debugging anything on the tablet. The second one is
the case that looks like a broken app and is not.

| | |
|---|---|
| Setting | `Api:Key` — on Azure, the app setting `Api__Key` (two underscores) |
| Minimum length | 32 characters. A shorter one throws at startup rather than being quietly weak. |
| Header | `X-Vms-Key: <the key>` |
| On the tablet | Settings → API key. Blank sends no header at all, which is right for the on-premises host. |

The on-premises host (`UATWEB01`) is reachable only from the office network and needs no key.
The internet-facing one does, and also has rate limiting applied to the group.

## Switching it on

One setting, and nothing else. The key grants `Vms.Officer`, and the `CanCheckIn` policy the
group requires accepts Officer, Supervisor, Admin and SystemAdmin — so the key on its own
satisfies it. No role assignment, no app registration, no second switch.

### 1. Generate the key where you will use it

Not in a chat window, not in an email, and not twice. A key that has been pasted anywhere it can
be read later is spent and has to be replaced. The command is in
[azure-deployment.md](azure-deployment.md#3-configure) — `RandomNumberGenerator`, 48 bytes,
64 base64 characters, and the application refuses anything under 32.

### 2. Set it on the server

Azure App Service → **Settings → Environment variables → App settings**:

| Name | Value |
|---|---|
| `Api__Key` | the generated key |

Two underscores, not a colon: that is how App Service spells `Api:Key`. Saving restarts the app.

Or from the CLI:

```bash
az webapp config appsettings set -g <resource-group> -n <app-name> --settings Api__Key="$key"
```

On the on-premises host it goes in `appsettings.Production.json`, which is gitignored, or in the
environment. UATWEB01 has never had one and does not need one — it is reachable only from the
office network.

### 2b. One key per tablet, so the report can say which one

With two desks, give each its own key and the report names the desk that recorded each visit.

| Name | Value |
|---|---|
| `Api__Keys__Reception 1` | a key generated for that tablet |
| `Api__Keys__Reception 2` | a different key |

The name is whatever you put after `Api__Keys__`, and it is what appears in the visit's
**Recorded by** column. Spaces are fine.

This is a credential rather than a label the tablet types about itself, which matters more than
it looks: a name typed into a settings screen works until somebody types the wrong one, or
copies a tablet's settings onto its replacement and leaves two desks both claiming to be desk
one. A key cannot be got wrong that way, and one tablet can be withdrawn without taking the
other down with it.

Two guards, both at startup rather than in a report months later:

- **The same key on two tablets is refused.** Both would record the first one's name, which is
  worse than no name at all — the report would be confidently wrong instead of silent.
- **A key under 32 characters is refused**, named so you know which one.

The plain `Api__Key` still works and still records `(not signed in)`, so a deployment already
using one keeps working unchanged. Set both if you are migrating: the old key keeps the desks
running while you move them across one at a time.

Startup names them:

```
Tablet keys configured: Reception 1, Reception 2.
```

### 3. Check the server before touching a tablet

Two checks, and both are worth doing in this order, because they separate a server problem from
a device problem.

The application states which of four situations it is in, once, at startup:

```
The /api endpoints are guarded by an Entra ID token or the tablet's API key
```

That is the line you want. Then prove it from anywhere:

```bash
curl -i -H "X-Vms-Key: $key" https://<host>/api/reference
```

| Response | What it means |
|---|---|
| `200` with entities and purposes | Working. Go to the tablet. |
| `401` | `Api__Key` is not set, or the key does not match. |
| `401` **without** the header too | Correct — that is the endpoint being guarded, not a fault. |
| `403` | Authenticated but refused by policy. Should not happen with the key; report it. |

### 4. Put the same key on the tablet

**Settings → API key**, then **Test connection** before **Save**. Testing first is the point of
that button: a wrong key saved turns every later screen into a failure with nothing on it naming
the cause.

### 5. Set a PIN on that screen, and restrict the API by IP

The key is now on a tablet that sits on a counter, and the screen you just typed it into will
show it to whoever taps the eye. Two things close that, and they close different halves of it.

**A PIN on the settings screen** — under the gear, 4 to 8 digits, set once per tablet. It is
asked for every time settings are opened, wrong tries are slowed to a doubling delay after
four, and there is no recovery: a forgotten PIN is cleared by clearing the app's data, which
clears the key and the address with it. See [android/README.md](../android/README.md#the-pin).
That stops the key being **copied**.

**IP access restrictions** — Azure App Service → Networking → Access restrictions, allowing the
office's egress addresses. That stops a copied key **mattering**, costs no code and no rebuild,
and is the stronger of the two. Do it whether or not the PIN is set.

The exposure being closed is not check-in: somebody holding the tablet can already check a
visitor in by using the app. It is `GET /api/people`, which returns names, titles, email
addresses and employers, and which a copied key reaches from anywhere on the internet.

## Authentication

Two ways in, one rule. The group requires the `Vms.Officer` check-in policy whichever scheme
answered, so the key is a way of arriving and not a way around the rules.

- **Desk browser** — Entra ID cookie for the pages, Bearer token for the API. Deliberately not
  the cookie: a session cookie stolen from a desk browser cannot be replayed against the API.
- **Tablet** — the `X-Vms-Key` header. Nobody signs in on a tablet sitting on a counter, so the
  key is the tablet's identity. Entries it records carry no person as their author, and the
  report shows `(not signed in)` rather than pretending otherwise.

## The endpoints

All under `/api`, all JSON, all relative to the address in the tablet's settings.

### `GET /api/reference`

Everything needed to draw the visit form, in one call. A tablet on office wifi is not a desk on
ethernet, and three round trips to draw one screen is three chances to be left halfway.

```json
{
  "entities":     [ { "id": 1, "name": "Dubai Investments PJSC" } ],
  "purposes":     [ "Meeting", "Delivery", "Interview", "Maintenance", "Other" ],
  "otherPurpose": "Other"
}
```

Only active entities, ordered by name.

### `GET /api/people?q=&entityId=&allEntities=`

```json
[ { "id": 0, "displayName": "Nayyar Ahmed", "title": "…", "companyName": "…",
    "directoryObjectId": "…" } ]
```

Two sources, and the tablet cannot tell which it got:

- **Entra ID**, when the tenant is configured. `id` is `0` and `directoryObjectId` carries the
  identity, because that person has no database row yet — one is written when a visit to them is
  saved.
- **`vms.Person`** otherwise. Needs two characters, returns at most twelve, and `entityId` narrows
  it unless `allEntities=true`.

A search term is required. This deliberately never returns the whole directory: names, titles,
email addresses and employers are not a thing to leave sitting on a tablet at a reception desk.

### `POST /api/reads`

No body.

```json
{ "requestId": "…" }
```

The server issues the ID, so a client cannot prepare a response before being asked for one. It
**expires after five minutes and is spent once** — that is what stops a captured card response
being replayed into a later check-in. Over that, the save is refused and the tablet returns to
step one with the visit details still filled in, so recovering is one tap and not a retyped form.

### `POST /api/visits`

```json
{
  "requestId":                 "…",
  "readResponseXml":           "<ValidationGatewayResponse>…</ValidationGatewayResponse>",
  "manual":                    null,
  "entityId":                  1,
  "personToVisit":             "Nayyar Ahmed",
  "personToVisitId":           null,
  "personToVisitDirectoryId":  "…",
  "purpose":                   "Meeting",
  "purposeOther":              null,
  "contactMobile":             "0501234567"
}
```

Either `readResponseXml` (a card read) or `manual` (typed details), not both. **Every card field
is parsed out of the signed XML on the server**, never taken from anything the client sent
beside it — a tablet claiming to have read a card is the same problem as a browser claiming it,
so it goes through the same `AgentCardReader` as the desk, with one set of rules and no second
way in.

`contactMobile` is nullable and last in the record on purpose: a tablet built before it existed
keeps working and simply sends nothing.

```json
{ "id": 10482, "recordedAtUtc": "2026-09-30T11:41:00Z",
  "captureMethod": "CardReaderUnverified", "warning": "…" }
```

`captureMethod` is the provenance the server assigned, not something the tablet asked for:
`CardReader`, `CardReaderUnverified` or `Manual`. On ICP's offline licence every chip read is the
middle one, and `warning` says so in words fit for the screen.

## The client

`VmsClient.kt` builds one Retrofit client per settings change. Worth knowing:

- **Timeouts** — 15s connect, 30s read, **60s write**. A card read posts signed XML with a
  photograph in it, over office wifi.
- **Unknown fields are ignored.** The server may grow a field before the tablets are updated, and
  a new field must not break a check-in.
- **Logging is headers and status only, never bodies**, and only in debug builds. A body here is
  an Emirates ID number, a date of birth and a photograph, which have no business in logcat on a
  device at a reception desk.
- **The address is normalised** before it is saved: a bare host gets `https://`, and a missing
  trailing slash is added — without it Retrofit silently drops the last path segment of the base
  URL.

## Checklist for a new tablet

1. `Api__Key` — or one `Api__Keys__<desk>` per tablet — set on the server, at least 32
   characters.
2. The startup log line reads **"an Entra ID token or the tablet's API key"**.
3. Settings on the tablet: server address, and the same key.
4. **Test connection** on that screen — it tries the address before saving, so a wrong one is
   refused there rather than turning every later screen into a connection failure.
5. **Set the PIN** on that same screen, and write it down somewhere that is not the tablet.
6. Confirm the app version under **This tablet** is the one you meant to install. The camera
   scanner and the PIN are 1.1.0; a tablet on 1.0.0 has neither, and a missing feature on an
   older APK looks exactly like a broken one.
