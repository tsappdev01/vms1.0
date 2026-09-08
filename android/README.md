# VMS Reception — Android

The reception app. It reads an Emirates ID at the desk and records the visit against the
same database the web app uses, through the same server.

This file is the engineering record for the Android side, the way
`src/DI.Vms.Blazor/README.md` is for the server. Read it before changing the card-reading
or sign-in paths — most of what is here was learned from the SDK's binaries and from real
cards, not from the documentation.

## What it is, and what it is not

It is one screen flow: **Insert card → Visitor → Visit → Done**, plus a way to type the
details in when a chip will not read.

It is not a second copy of the system. The tablet has no database connection and no
business rules of its own:

- Every field about a visitor comes from `Api/VisitsApi.cs` on the server, parsed there
  out of the signed card response. Nothing the tablet says about the visitor is believed.
- The host directory is searched on the server. It is 725 people with titles, email
  addresses and employers, and that is a staff directory that should not be sitting on a
  tablet at a reception desk.
- The purposes and the entity list come from `GET /api/reference`, so they change when the
  server changes and not when the app is rebuilt.

## The reader: an ACS reader on USB, not NFC

The desks already have ACR39U readers and reception already has the habit of putting a
card into one, so that is what this uses — over USB-OTG on the tablet.

NFC is in the toolkit and it is tempting, but it is **not** tap-and-go. `CardReader`
exposes `setNfcAuthenticationParameters(cardNumber, dob, expiryDate)`, and without those
three values the chip releases nothing over the air — it is a BAC-style access key derived
from what is printed on the card. So an NFC flow has to get the card number, the date of
birth and the expiry date first, by OCR or by typing, and that is more steps at the desk
than inserting the card, not fewer.

`card/CardRead.kt` therefore puts the read behind `interface EmiratesIdReader`. If NFC is
ever wanted, it goes in as a second implementation and nothing above it changes.

## What the SDK actually does

Verified against `id-card-toolkit-android-sdk-v3.1.6/lib/EIDAToolkit.aar` and ICP's
`samples/ToolkitSample`, because several of these are not what the obvious guess would be:

| Thing | The fact |
| --- | --- |
| Configuration | Newline-separated `key = value`, **not** JSON. JSON is rejected with "Invalid or incomplete configuration data". |
| `plugin_directory_path` | `context.applicationInfo.nativeLibraryDir + "/"`. The ACS driver's `.so` is unpacked there at install time by the `:acs-plugin` module — it is not a path somebody populates on the device. |
| Where the config lives | Extracted from the APK to `filesDir`. ICP's sample uses external storage, which on any Android this app targets means a permission prompt reception should not be answering. |
| The signed response | `CardPublicData` extends `ToolkitResponse`, and **`toXmlString()`** is the whole signed Validation Gateway document. `getResponseDataElement()` is one element inside it and is `protected` anyway. Posting the wrong one means every read is rejected server-side. |
| The photograph | `getCardHolderPhoto()` returns **base64 text**, not bytes. |
| Toolkit version | The accessor is `getToolkitVerison()` — ICP's spelling. Kotlin's synthetic property keeps the typo, so `kit.toolkitVerison` is correct and `toolkitVersion` does not compile. |
| Gson | `EIDAToolkit.aar` references `com.google.gson.*` but does not bundle it. `EIDAToolkit/build.gradle.kts` declares it on the module's `default` configuration so it propagates transitively; a plain `implementation(files(...))` cannot, and the build then fails at `DexingNoClasspathTransform` naming the AAR rather than the missing dependency. |
| Spongy Castle | The toolkit's PKI code is built against `org.spongycastle`. Android's own cut-down `org.bouncycastle` provider is not a substitute. |
| ABI | arm64-v8a and armeabi-v7a only. Hence the `abiFilters`: without it an x86_64 emulator installs happily and fails the first read with an `UnsatisfiedLinkError`, which reads like a code fault. |

## Reads are unsigned, and the app says so

The licence ICP issued is an offline bundle, so `read_publicdata_offline = true` and the
response never reaches the Validation Gateway to be signed. The server accepts it with
`RequireSignature: false` and marks the record `CardReaderUnverified`; the tablet shows
the server's warning on the Done screen, and the report labels it **Card (unverified)**.

This is the interim, not the destination — see `docs/icp-signed-response-request.md`. When
ICP issues an online licence, `TrustedSignerThumbprints` gets pinned on the server and
nothing in this app changes.

## Two things are not in this repository

### `app/src/main/res/raw/auth_config.json`

MSAL's configuration. It carries the client and tenant IDs of the Entra app registration,
and `auth/MsalConfig.kt` reads the client ID back out of it so the API scope
(`api://<client id>/Visits.Write`) cannot drift from a second copy of the GUID.

`auth_config.template.json` in this directory is the file to copy:

```bash
cp auth_config.template.json app/src/main/res/raw/auth_config.json
```

Until it exists the build fails on `R.raw.auth_config`, which is the intended outcome —
an app that compiles without a tenant would install and then fail at the desk instead.
Filled in, it looks like this:

```json
{
  "client_id": "<application (client) ID from the app registration>",
  "authorization_user_agent": "DEFAULT",
  "redirect_uri": "msauth://ae.dubaiinvestments.vms/<url-encoded signature hash>",
  "account_mode": "SINGLE",
  "broker_redirect_uri_registered": false,
  "authorities": [
    {
      "type": "AAD",
      "audience": {
        "type": "AzureADMyOrg",
        "tenant_id": "<directory (tenant) ID>"
      }
    }
  ]
}
```

`account_mode` must be `SINGLE`. `auth/Auth.kt` uses the single-account client on purpose:
a reception tablet is one desk with one signed-in officer, and the multiple-account client
offers an account picker on every token request — a prompt with a visitor waiting.

### `app/src/main/assets/toolkit-config/`

ICP's configuration bundle for this deployment, licence file included. It is not ours to
publish. Copy the same directory the desk agent's MSI is given as `CONFIG_DIRECTORY`
(see `deploy/install-desk-agent.ps1`); `card/ToolkitConfig.kt` looks for `config_li` and
names the omission rather than letting the toolkit report "invalid or incomplete
configuration data".

Both are in `.gitignore`. Neither is a secret in the sense a password is, but neither
belongs in a public repository either.

## Entra ID

The server side is in `docs/entra-id-setup.md`. The app needs three additions to the same
registration:

1. **Expose an API** → Add a scope `Visits.Write`. The Application ID URI must be
   `api://<client id>`, which is the default. This is the scope the tablet asks for and
   the audience the server checks — a Graph token will not do.
2. **Authentication → Add a platform → Android**, package name `ae.dubaiinvestments.vms`,
   signature hash from the command below. This produces the
   `msauth://ae.dubaiinvestments.vms/<hash>` redirect URI.
3. **App roles** — the same `Reception` role the web app uses. The API checks it through
   `VmsRoles.CanCheckIn`, and a signed-in user without it gets a 403 and is told to ask
   IT for the role.

The signature hash, for the debug keystore:

```bash
keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore \
    -storepass android -keypass android \
  | openssl sha1 -binary | openssl base64
```

Use the release keystore's alias for the release build. The hash goes in three places and
they must agree: the Entra platform registration, `redirect_uri` in `auth_config.json`,
and `MSAL_SIGNATURE_HASH` for the manifest (`gradle.properties`, or
`-PMSAL_SIGNATURE_HASH=...`). Unset, the build succeeds, installs, and then fails the
return leg of sign-in with a redirect mismatch — which is why the default is the visibly
wrong `MSAL_SIGNATURE_HASH_NOT_SET` rather than an empty string.

## Building

Android Studio Ladybug or later, JDK 17. From this directory:

```bash
./gradlew :app:assembleDebug
```

`gradle.properties` holds the two settings that vary:

- `SDK` — where ICP's Android SDK sits. It defaults to the copy already in this
  repository, and the three shim modules read it, so the binaries are referenced from one
  place instead of copied into three.
- `VMS_API_BASE_URL` — the server, `https://vms.dipark.com/` by default. The trailing
  slash matters to Retrofit; without it Retrofit drops the last path segment.

## Each tablet needs registering with ICP

The toolkit ties a licence to a device. A new tablet has to be registered
(`Toolkit.registerDevice`) or its reads will be refused with a licence error, however
correct the configuration is. Raise it with ICP with the device ID from
`Toolkit.getDeviceId()` before a tablet goes to a desk.

## The five-minute read

`AgentCardReader.BeginRead()` issues a request ID that expires after five minutes and is
spent once. That is what stops a captured response being replayed into a later check-in,
and it is the same window the desk browser works in.

If an officer takes longer than that between reading the card and pressing **Record the
visit**, the server refuses the save and says so. The app then goes back to step one **with
the visit details still filled in**, so recovering is one tap on Read card and not a
re-typed form. `VisitorViewModel.save()` is where that is handled; it is deliberate, not a
side effect.

## What is not built yet

- **No offline queue.** A tablet with no network cannot record a visit. Adding one would
  mean holding a signed card response — an Emirates ID number, a date of birth and a
  photograph — on the device until the network came back, which is a worse problem than
  the one it solves. If reception needs it, the queue should hold the *visit* and re-read
  the card, not store the identity.
- **No visitor report on the tablet.** The web app has it, and reception has a browser.
- **No check-out.** The system records arrivals; nothing in the database represents a
  departure yet, on any client.
