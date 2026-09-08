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

## Sign-in is currently off

`VMS_AUTH_ENABLED=false` in `gradle.properties`, matching the server's
`Authentication:Enabled`. **The two must agree** — a tablet sending no token to a server
that requires one gets a 401 on every screen, and the reverse is a prompt for nothing.

While it is off:

- MSAL is never initialised, so **`res/raw/auth_config.json` is not needed** and the app
  builds and runs from a clean clone. `auth/MsalConfig.kt` looks the resource up by name
  rather than as `R.raw.auth_config` precisely so the build does not insist on a file it
  will not open.
- No token is attached to any request — `VmsClient` does not even install the interceptor.
- The top bar says **Sign-in is off** where the officer's name goes, and the server
  records every visit against `(not signed in)`.

To turn it on: put `auth_config.json` in place, finish the directory work in
`docs/entra-id-setup.md`, then build with `-PVMS_AUTH_ENABLED=true` and set the server's
`Authentication:Enabled` to true. None of the sign-in code was removed to get here.

## Two things are not in this repository

### `app/src/main/res/raw/auth_config.json`

MSAL's configuration. It carries the client and tenant IDs of the Entra app registration,
and `auth/MsalConfig.kt` reads the client ID back out of it so the API scope
(`api://<client id>/Visits.Write`) cannot drift from a second copy of the GUID.

`auth_config.template.json` in this directory is the file to copy:

```bash
cp auth_config.template.json app/src/main/res/raw/auth_config.json
```

It is only needed for a build with `-PVMS_AUTH_ENABLED=true`. Without it, a sign-in
build fails at startup with the message `MsalConfig.kt` writes, naming this file — rather
than at the desk with a redirect error.

The client and tenant IDs in the template are the real ones
(`docs/entra-id-setup.md` records the registration). The **signature hash is the one
value to fill in**, because it belongs to the signing keystore rather than to the source.
Filled in, the file looks like this:

```json
{
  "client_id": "dd0fec3e-2476-4823-a73b-7706c5f8ce7e",
  "authorization_user_agent": "DEFAULT",
  "redirect_uri": "msauth://ae.dubaiinvestments.vms/<url-encoded signature hash>",
  "account_mode": "SINGLE",
  "broker_redirect_uri_registered": false,
  "authorities": [
    {
      "type": "AAD",
      "audience": {
        "type": "AzureADMyOrg",
        "tenant_id": "ba42ffd1-f322-49fa-81b7-74dcbd5f52a7"
      }
    }
  ]
}
```

`account_mode` must be `SINGLE`. `auth/Auth.kt` uses the single-account client on purpose:
a reception tablet is one desk with one signed-in officer, and the multiple-account client
offers an account picker on every token request — a prompt with a visitor waiting.

**There is no client secret in this file and there must never be one.** The tablet is a
public client: anything shipped in an APK is readable by anyone holding the APK, so a
secret there is a published secret. The server's secret belongs only in
`appsettings.Production.json` on UATWEB01.

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
3. **No new app roles.** The API requires the `CanCheckIn` policy, which
   `Vms.Officer`, `Vms.Supervisor`, `Vms.Admin` and `Vms.SystemAdmin` already satisfy — so
   anyone who can check a visitor in on the web can do it on the tablet. A signed-in user
   with none of them gets a 403 and is told on screen to ask IT for the role.

### Getting the signature hash

Entra wants the base64 hash. The manifest and `auth_config.json` want the same value
**URL-encoded** — base64 contains `+`, `/` and `=`, and those have to be escaped inside a
URI. This is the usual reason a first sign-in attempt fails.

On Windows, in PowerShell from this directory (`keytool` comes with the JDK Android
Studio installs, so add it to `PATH` or call it by full path):

```powershell
$store = "$env:USERPROFILE\.android\debug.keystore"   # release: your own keystore
$alias = 'androiddebugkey'                            # release: your own alias
$pass  = 'android'                                    # release: your own store password

$sha1 = (keytool -list -v -alias $alias -keystore $store -storepass $pass |
    Select-String 'SHA1:' | Select-Object -First 1).ToString().Split(':', 2)[1].Trim()

$hash = [Convert]::ToBase64String([byte[]]($sha1 -split ':' | ForEach-Object { [Convert]::ToByte($_, 16) }))

"Entra portal      : $hash"
"auth_config / hash: $([uri]::EscapeDataString($hash))"
```

`./gradlew signingReport` prints the same SHA1 for every variant if you would rather read
it that way.

On Linux or macOS the one-liner is:

```bash
keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore \
    -storepass android -keypass android \
  | openssl sha1 -binary | openssl base64
```

The URL-encoded hash then goes in three places, and they must agree:

| Where | Which form |
|---|---|
| Entra → Authentication → Android platform | base64, as printed |
| `redirect_uri` in `res/raw/auth_config.json` | URL-encoded |
| `MSAL_SIGNATURE_HASH` in `gradle.properties` (or `-PMSAL_SIGNATURE_HASH=…`) | URL-encoded |

Leave the Gradle property unset and the build succeeds, installs, and then fails the
return leg of sign-in with a redirect mismatch. That is why the default is the visibly
wrong `MSAL_SIGNATURE_HASH_NOT_SET` rather than an empty string that looks plausible in a
manifest.

## Building

There is nothing to compile file by file — Kotlin for Android is built by Gradle, which
compiles the sources, runs the Compose compiler plugin, merges the resources, dexes
everything and packages the APK. One command does all of it.

**What has to be installed**

| | |
|---|---|
| Android Studio | Ladybug (2024.2) or **any later release**. It brings its own JDK, the SDK manager and `adb`. |
| Android SDK | Platform **35** and the current build-tools, from Tools → SDK Manager. `compileSdk = 35`. |
| Internet | The first build downloads Gradle 8.14.2, AGP, Compose and MSAL — roughly 1 GB into `%USERPROFILE%\.gradle`. Later builds are offline-ish and quick. |

A newer Android Studio is not a problem in itself, but it brings two things worth knowing
about, because both are the IDE trying to be helpful rather than anything wrong with the
project:

**Its bundled JDK has to be one Gradle 8.14.2 can run on** — that is Java 8 to 24, so
JDK 17 or 21 is the safe setting and a very new release bundling something later would be
refused with a message about an unsupported class file or JVM version. Check what Gradle
is actually using:

```powershell
.\gradlew.bat -version
```

If the JVM line is beyond 24, point the IDE at a supported one in **Settings → Build,
Execution, Deployment → Build Tools → Gradle → Gradle JDK** (17 or 21). That setting is
per-project and belongs to the IDE, not to this repository.

**Decline the "upgrade AGP" prompt** the first time it appears. AGP 8.7.3 and Gradle
8.14.2 are a pair that is known to work here; upgrading AGP pulls the Gradle wrapper with
it, and there is no reason to change both at once while the first build has not passed.
The same goes for a suggestion to raise `compileSdk` to 36 — harmless to ignore.

**The one file Gradle needs that is not in git**

`android/local.properties`, saying where the SDK is. Android Studio writes it the first
time it opens the project; by hand it is one line:

```properties
sdk.dir=C\:\\Users\\<you>\\AppData\\Local\\Android\\Sdk
```

Backslashes are escaped because it is a Java properties file. `ANDROID_HOME` works instead
if it is already set.

**Then, from `C:\Claude.AI\vms1.0\android` in PowerShell**

```powershell
.\gradlew.bat :app:assembleDebug
```

The APK lands at `app\build\outputs\apk\debug\app-debug.apk`. Onto a tablet with USB
debugging on:

```powershell
adb install -r app\build\outputs\apk\debug\app-debug.apk
```

Or just open `C:\Claude.AI\vms1.0\android` in Android Studio and press Run — same build,
and it installs and attaches logcat for you.

**When it fails**, these are the useful flags: `--stacktrace` for where, and
`.\gradlew.bat :app:assembleDebug --info 2>&1 | Tee-Object build.log` for a log worth
reading afterwards. `.\gradlew.bat clean` and `.\gradlew.bat --stop` (kills the daemon)
between attempts when something is stuck.

**Settings in `gradle.properties`**

- `SDK` — where ICP's Android SDK sits, relative to this directory. Read by the three shim
  modules through `rootProject.file`, so the binaries are referenced from one place
  instead of copied into three. A wrong value fails the build naming the file it looked
  for.
- `VMS_API_BASE_URL` — the server, `https://vms.dipark.com/` by default. The trailing
  slash matters to Retrofit; without it Retrofit drops the last path segment.
- `VMS_AUTH_ENABLED` — sign-in, currently `false`. Must match the server's
  `Authentication:Enabled`.

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
