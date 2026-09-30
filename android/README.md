# VMS Reception — Android

The reception app. It reads an Emirates ID at the desk and records the visit against the
same database the web app uses, through the same server.

This file is the engineering record for the Android side, the way
`src/DI.Vms.Blazor/README.md` is for the server. Read it before changing the card-reading
path — most of what is here was learned from the SDK's binaries and from real cards, not
from the documentation.

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

## Pointing the app at a laptop for testing

The quickest way to exercise the app is to run the server on your own machine and have the
tablet talk to it over the office wifi. Three things have to be true, and only the third
needs anything from this repository.

**Bind Kestrel to every interface.** `launchSettings.json` says `localhost`, which listens
on 127.0.0.1 and nowhere else:

```powershell
cd src\DI.Vms.Blazor
dotnet run --urls "http://0.0.0.0:7100;https://0.0.0.0:7101"
```

**Let the connection in** - once, elevated:

```powershell
New-NetFirewallRule -DisplayName "VMS dev 7100/7101" -Direction Inbound `
  -Protocol TCP -LocalPort 7100,7101 -Action Allow -Profile Private
```

**Use plain HTTP from the app.** The ASP.NET development certificate is issued for
`localhost`, so `https://<laptop ip>:7101` fails the hostname check on the tablet. A
browser offers to continue; an app has nobody to ask, and no server-side setting fixes it -
the name in the certificate is not the name being used. So debug builds allow cleartext,
through `app/src/debug/res/xml/network_security_config.xml`.

That file is in `src/debug/`, which means it is compiled into the debug APK and cannot
reach a release one. Release builds keep Android's defaults: cleartext refused, only system
certificate authorities trusted. Production hosts - `vms.dipark.com` and any Azure Web App -
carry certificates from a public CA and need none of this.

```powershell
.\gradlew.bat :app:assembleDebug --console=plain `
  -PVMS_API_BASE_URL="http://192.168.1.188:7100/"

& $adb install -r app\build\outputs\apk\debug\app-debug.apk
```

The laptop's address, the HTTP port, and the trailing slash. Check it from the tablet's
browser first - `http://192.168.1.188:7100` should show the reception screens. If that
fails, the firewall or the binding is the problem and the app will not do better.

## Nobody signs in

There is no sign-in screen, no MSAL and no `auth_config.json`. The app opens on step one.

That is a decision about what the tablet *is*. It sits on a counter, it is handed to
nobody, and it is in the room the visitors are in — so it is not a person's device and it
cannot carry a person's credential. It identifies itself to the server with an API key
instead, which is why every visit is recorded against `(not signed in)` rather than against
an officer, and why the key is a **tablet** credential: anyone holding the APK, or the
tablet, has it.

What follows from that:

- The server's `Authentication:Enabled` can be `true` — that is the intended deployment:
  sign-in for the web screens, the key for the tablet. `/api` names both the bearer scheme
  and the key scheme and then applies the same `CanCheckIn` policy, so the key is a way of
  arriving, not a way around the rules. What the tablet must have is `Api:Key` set on the
  server; with sign-in on and no key configured, nothing here can reach the API and the app
  says so.
- The API key is the whole of the tablet's access. Rotate it by changing it on the server
  and typing the new one into **Settings** on each tablet — no rebuild.
- **The settings screen is therefore worth locking**, and a PIN does it — see below. The key
  works from anywhere on the internet, and `GET /api/people` behind it returns the staff
  directory, so a key copied off a counter is an exposure that does not end at the door.
- The top bar shows **which server** the tablet is talking to, in the place the officer's
  name used to be. It is the one thing about a reception tablet that can quietly be wrong.

The sign-in code (`auth/Auth.kt`, `auth/MsalConfig.kt`, the MSAL dependency and the
`BrowserTabActivity` in the manifest) was removed rather than left switched off. It is in
the git history if a tablet ever needs a signed-in officer; `docs/entra-id-setup.md` covers
the server side, which is unaffected.

## Settings, on the tablet

The gear in the top bar, behind a PIN once one is set. Two fields, and both of them are
addressed to whoever installs a tablet rather than to reception:

- **Server address** — where visits are sent. It defaults to what the build was made with
  (`VMS_API_BASE_URL`), currently `https://vmsdi.dubaiinvestments.com/`. A bare host is accepted
  and `https://` is added; a missing trailing slash is added too, because without it
  Retrofit silently drops the last path segment of the base URL.
- **API key** — what the tablet identifies itself with. Blank is right for the
  on-premises host, which is reachable only from the office network and asks for none.

**Test connection** tries the typed address before it is saved, against a client of its
own, so a test cannot leave the tablet pointed somewhere it was not meant to go. Saving
clears the entity and purpose lists and reloads them, because those belong to the server
that was just replaced.

### The PIN

`settings/DeskPin.kt`, and the reason it exists is the field above it: the API key is stored
on the tablet and the screen will show it on request. Everything else this app does is meant
to be done by whoever is holding it. This is the one thing that is not.

- **Set it under the gear**, 4 to 8 digits. Until one is set the card says so in red, because
  "anyone can read the key" is a fact worth stating rather than a feature worth suggesting.
- **It guards the settings screen only** — not the app, not a check-in. A visitor being
  checked in never meets it.
- **Asked for on every opening.** Closing the screen re-locks it, by the back arrow or by
  saving; so does the app restarting. A PIN asked for once a day is not a PIN.
- **Changing or removing it needs the current one**, although the screen is already open. The
  case that guards is not somebody who got past the gate — it is a settings screen left open
  on the counter while reception answers the phone.
- **Wrong tries are slowed down, and the count is on disk.** Four are free; after that the
  field closes for 30 seconds, doubling to a quarter of an hour. Force-stopping the app is
  the obvious way to reset a counter and it does not work.
- **A hash is stored, never the PIN** — PBKDF2-HMAC-SHA256 where the tablet's provider has
  it, SHA-1 where it does not, fresh salt, 60,000 rounds, verified off the main thread. Worth
  being plain about what that buys: anyone who can read the preferences file can read the API
  key sitting in it, so the hash is there so the file does not spell the PIN out, not because
  it has to survive cracking. What stops a PIN being guessed is the lockout.
- **There is no recovery.** No master PIN, no reset code — either would be the thing worth
  attacking. A forgotten PIN is cleared by clearing the app's data in Android settings, which
  clears the server address and the key with it: the only way past the PIN destroys what the
  PIN was protecting. Write it down somewhere that is not the tablet.

A PIN is not the strongest control available here and is not meant to be the only one.
**Azure App Service → Networking → Access restrictions**, allowing the office's egress
addresses, makes a copied key worthless from outside the building and needs no code at all.
The PIN stops the copying; the IP restriction stops it mattering.

The address used to be compiled in, which meant moving a tablet to another server was a
rebuild — and the person who needs to do that is standing in front of the tablet with a
reader plugged into it, not in front of Android Studio. `settings/Settings.kt` holds it in
`SharedPreferences`, which `android:allowBackup="false"` keeps out of cloud backups and
device-to-device transfers. `api/ApiProvider.kt` rebuilds the Retrofit client when, and
only when, the address changes.

## What the tablet checks before it sends

`ui/FieldRules.kt`, and it is a **copy**. The rules belong to the server, in
`Services/VisitorFields.cs`, and that is where a save is actually judged — this exists because
a tablet cannot call C# and because being told after a round trip is being told too late: the
officer has moved on, the visitor is waiting, and the message arrives attached to a save rather
than to a field.

The two are allowed to differ in exactly one direction. The tablet may be **quieter** than the
server, since the server has the final say either way; it must never refuse something the server
would take, which would strand the desk with no way forward.

**One field blocks: the Emirates ID number**, when it was typed. Fifteen digits, beginning 784,
and the fifteenth is a Luhn checksum over the first fourteen — so a single wrong digit, and
almost every swap of two adjacent ones, is caught at the desk rather than in a report months
later. It is what a repeat visit is matched on, which is why it is the one worth stopping for: a
wrong number does not make a bad record, it makes a second person. A chip read and a
photographed card are not re-judged, because the card is the authority on what the card says.

**Everything else is said, not enforced:** a telephone number with letters in it or a UAE mobile
a digit out, a date the report will not be able to read, and a card that has already expired.
All worth putting in front of the officer, none worth refusing a visitor over — refusing here
sends reception for a pen and paper.

Also here: **every box is capped** at the width of the column behind it, which the tablet did
not do at all before. A long paste used to reach the server and be cut to fit in silence.

Two things were made to agree with the desk browser rather than with themselves:

- **The mobile number is required**, and the field says so. It was marked *optional* here while
  the desk browser required it — the kind of disagreement that only shows up later as a visitor
  nobody can ring.
- **What is still missing is named**, above the button, instead of the button simply being grey.
  A disabled button with nothing beside it is the single most common reason a desk rings
  somebody.

## One thing is not in this repository

### `app/src/main/assets/toolkit-config/`

ICP's configuration bundle for this deployment, licence file included. It is not ours to
publish. Copy the same directory the desk agent's MSI is given as `CONFIG_DIRECTORY`
(see `deploy/install-desk-agent.ps1`); `card/ToolkitConfig.kt` looks for `config_li` and
names the omission rather than letting the toolkit report "invalid or incomplete
configuration data".

It is in `.gitignore`. It is not a secret in the sense a password is, but it does not
belong in a public repository either.

## What ICP's build instructions require, and what was missing

Checked line by line against *ID Card Toolkit Android Sample Build Instructions v1.6* section
2.3, and against ICP's own `samples/ToolkitSample/app/build.gradle`, which is the working
configuration rather than a description of one. Six requirements were absent, and **every one
of them fails at runtime rather than at build time** — so the build was green, the APK
installed, and a card read would have failed at a desk.

| Required | Why it is not optional |
|---|---|
| `jniLibs { useLegacyPackaging = true }` | The toolkit loads its reader plugins with `dlopen` against a filesystem path. From AGP 4.1 native libraries stay compressed inside the APK, where there is no path to open, and the load returns "library not found". |
| `doNotStrip` every `.so` (`keepDebugSymbols` in AGP 8) | Gradle re-strips native libraries as it packages them, and a re-stripped plugin no longer matches the checksum the toolkit verifies it against. |
| `pickFirst` on both `libc++_shared.so` | Two plugin AARs ship it built against different NDK releases; duplicate paths fail the merge. |
| `multiDexEnabled true` | The toolkit and its plugins pass the 64K method limit on their own. |
| `minSdk 28` | ICP's floor. It was 26 here, which is not "older" but untested — and a tablet that installs and then fails at the reader is worse than one that refuses the install. Costs Android 8.0 and 8.1. |
| `exclude 'AndroidManifest.xml'` from packaged resources | Several plugin AARs carry one. Nothing to do with the manifest merger, which has already run by then. |

`android:extractNativeLibs="true"`, which the document also asks for, is set by AGP from
`useLegacyPackaging` — with both set AGP takes the DSL and warns about the manifest, so it is
set in one place only.

`android:requestLegacyExternalStorage="true"` is deliberately **not** set. ICP's sample reads
its configuration from external storage and needs it; this app extracts the bundle into its own
private storage, which is what the same document recommends as the long-term approach and what
`card/ToolkitConfig.kt` does.

One place this project still differs from ICP on purpose: their sample does not minify at all,
and this one does for release, with keeps for the toolkit, the ACS driver, Spongy Castle and
xmlsec — all of which are reached reflectively and none of which R8 can see is used. CI builds
debug, so that path is unexercised; treat the first release build as untested.

## Building without installing anything

`.github/workflows/android.yml` builds the APK on a push that touches `android/`, and can be
run on demand from the repository's **Actions** tab. The APK comes back attached to the run as
**vms-reception-apk** — download it, copy it to the tablet, install it. Nobody needs Android
Studio to get one.

**Nothing has to be set up for this.** ICP's configuration bundle is committed at the
repository root (`IDCARDOFFLINE_config_2026-04-14/`), and the workflow copies it into the APK's
assets. The copy under `android/app/src/main/assets/toolkit-config/` is gitignored only so the
same six files are not in git twice; the root copy is the one that matters.

Two optional repository secrets:

| Secret | What it is for |
|---|---|
| `TOOLKIT_CONFIG_B64` | A **renewed** bundle that is not to be committed — base64 of a gzipped tar of the directory's *contents*. Overrides the committed one when set. |
| `VMS_API_KEY` | Only a default; the key can also be typed into the settings screen on the tablet. |

```bash
tar cz -C IDCARDOFFLINE_config_2026-04-14/IDCARDOFFLINE_ag_config_2026-04-14 . | base64 -w0
```

Either way the workflow checks that `config_li` is there afterwards and fails naming it if not,
so a bundle packed one directory too high is caught in CI rather than on a tablet at a desk.

**The licence runs to 14 April 2027.** The `2026-04-14` in the bundle's name is when it was
issued, not when it expires — `docs/icp-signed-response-request.md` records the expiry the
toolkit itself reported. It is a **PRE-PRODUCTION, offline** bundle, which is why reads come
back unsigned and are recorded as `CardReaderUnverified`.

One thing worth a decision rather than a silent assumption: this bundle carries the licence
issued to Dubai Investments and it is committed here. That is fine for a private repository and
not fine for a public one, and it is already in the history, so making the repository public
later would publish it retrospectively.

## Building

There is nothing to compile file by file — Kotlin for Android is built by Gradle, which
compiles the sources, runs the Compose compiler plugin, merges the resources, dexes
everything and packages the APK. One command does all of it.

First green build: 2026-09-09, on Android Studio Quail with JDK 21, AGP 8.7.3 and Gradle
8.14.2. Everything below was learned getting there, in the order it bites.

**What has to be installed**

| | |
|---|---|
| Android Studio | Ladybug (2024.2) or **any later release**. It brings its own JDK, the SDK manager and `adb`. |
| Android SDK | Platform **35** and the current build-tools, from Tools → SDK Manager. `compileSdk = 35`. |
| Internet | The first build downloads Gradle 8.14.2, AGP, Compose and MSAL — roughly 1 GB into `%USERPROFILE%\.gradle`. Later builds are offline-ish and quick. |

A newer Android Studio is not a problem in itself, but it brings two things worth knowing
about, because both are the IDE trying to be helpful rather than anything wrong with the
project:

**Its bundled JDK is probably too new, and the build fails in about a second.** Gradle
8.14.2 runs on Java 8 to 24. Android Studio Quail bundles **JDK 25**, so out of the box the
wrapper dies with a Gradle-internal stack trace and nothing that names the cause. Check
first — the `Launcher JVM:` line is the answer:

```powershell
.\gradlew.bat -version
```

Anything up to 24 is fine. Beyond that, install a JDK 21 and point both the terminal and
the IDE at it:

```powershell
winget install --id Microsoft.OpenJDK.21 --accept-package-agreements --accept-source-agreements

$jdk = (Get-ChildItem 'C:\Program Files\Microsoft' -Directory -Filter 'jdk-21*' | Select-Object -First 1).FullName
[Environment]::SetEnvironmentVariable('JAVA_HOME', $jdk, 'User')
$env:JAVA_HOME = $jdk
```

And in **Settings → Build, Execution, Deployment → Build Tools → Gradle → Gradle JDK**,
which has a "Download JDK" option if winget is not wanted. The IDE ignores `JAVA_HOME` and
uses that setting, so both have to be changed or the IDE keeps failing the same way.

Upgrading Gradle instead is not the cheap option it looks like: Java 25 needs Gradle 9.x,
which needs AGP 8.13 or later, which needs a newer Kotlin Gradle plugin. That is a
three-way version bump, and worth doing deliberately rather than to get past a JDK
mismatch.

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

**`JAVA_HOME is not set and no 'java' command could be found in your PATH`**

Expected, on a machine where Android Studio is the only Java. It carries its own JetBrains
Runtime and uses it from inside the IDE without ever touching `JAVA_HOME`, so the wrapper
run from an ordinary terminal finds nothing. Three ways out, easiest first:

1. Build from the IDE, or from **its** terminal (Alt+F12), which inherits its environment.
2. Set it for one cmd window: `set JAVA_HOME=C:\Program Files\Android\Android Studio\jbr`
3. Set it for the account, once:
   ```powershell
   [Environment]::SetEnvironmentVariable('JAVA_HOME','C:\Program Files\Android\Android Studio\jbr','User')
   ```
   Then open a new terminal.

Older installs name it `jre` rather than `jbr`. To find whichever it is:

```powershell
Get-ChildItem 'C:\Program Files\Android' -Directory -Recurse -Depth 2 -Filter 'jbr' `
  -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
```

**On the Dubai Investments network, Java needs the Zscaler root CA**

Zscaler decrypts and re-signs HTTPS. Its root is in the Windows certificate store, so
PowerShell and browsers are happy; it is not in the JDK's `cacerts`, so Gradle sees a
broken chain. The symptom is not a certificate error - Gradle's plugin resolver swallows
it and reports

```
Plugin [id: 'com.android.application', version: '8.7.3'] was not found in any of the
following sources: ... Searched in the following repositories: Google, MavenRepo, ...
```

which sends you looking for a wrong version number. Confirm it by asking Java itself what
certificate the host serves - `-printcert` prints the chain without validating it, so it
works even when trust fails:

```powershell
& "$env:JAVA_HOME\bin\keytool.exe" -printcert -sslserver dl.google.com:443 |
  Select-String -Pattern 'Owner:|Issuer:'
```

An `Issuer` naming Zscaler rather than Google Trust Services is the confirmation. The fix
is to export the proxy's certificates and import them into the JDK truststore. Export
needs no privileges:

```powershell
$certs = @(Get-ChildItem Cert:\LocalMachine\Root, Cert:\CurrentUser\Root |
    Where-Object { $_.Subject -like '*Zscaler*' })

$dir = "$env:USERPROFILE\zscaler-ca"
New-Item -ItemType Directory -Force -Path $dir | Out-Null

$n = 0
foreach ($c in $certs) {
    $n++
    $p = Join-Path $dir "zscaler-$n.cer"
    Export-Certificate -Cert $c -FilePath $p -Type CERT | Out-Null
    "$p  <-  $($c.Subject)"
}
```

The import does, because it writes inside `Program Files` - run PowerShell as
administrator:

```powershell
Get-ChildItem "$env:USERPROFILE\zscaler-ca\*.cer" | ForEach-Object {
    & "$env:JAVA_HOME\bin\keytool.exe" -importcert -cacerts -storepass changeit -noprompt `
        -alias $_.BaseName -file $_.FullName
}
```

It is per JDK, not per project, so it is done once per machine - and again for any other
JDK later installed. `.\gradlew.bat --stop` afterwards, so the next build starts a daemon
that reads the new truststore.

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

**`e: The daemon has terminated unexpectedly on startup attempt #1`** during
`compileDebugKotlin` is the Kotlin compile daemon, and it usually recovers by itself on
attempt #2 - the line looks fatal and is not, so read on to the end before acting on it.
If it does exhaust its attempts, run the compiler inside the Gradle daemon instead, in
`%USERPROFILE%\.gradle\gradle.properties` (not this repository - it is a property of the
machine, and user-level Gradle properties override the project's):

```properties
kotlin.compiler.execution.strategy=in-process
org.gradle.jvmargs=-Xmx4096m -Dfile.encoding=UTF-8
```

The heap goes up because the compiler now shares the Gradle daemon's. The underlying cause
is usually endpoint security interfering with the daemon's local socket, so an antivirus
exclusion for `%USERPROFILE%\.gradle` and the project folder is the better fix where it
can be had - it makes every build faster too.

**When it fails**, these are the useful flags: `--stacktrace` for where, and
`.\gradlew.bat :app:assembleDebug --info 2>&1 | Tee-Object build.log` for a log worth
reading afterwards. `.\gradlew.bat clean` and `.\gradlew.bat --stop` (kills the daemon)
between attempts when something is stuck.

**Settings in `gradle.properties`**

- `SDK` — where ICP's Android SDK sits, relative to this directory. Read by the three shim
  modules through `rootProject.file`, so the binaries are referenced from one place
  instead of copied into three. A wrong value fails the build naming the file it looked
  for.
- `VMS_API_BASE_URL` — the server a build points at *by default*;
  `https://vmsdi.dubaiinvestments.com/` unless overridden. The
  tablet can be moved elsewhere from the settings screen, so this only decides where a
  fresh install looks first. The trailing slash matters to Retrofit; without it Retrofit
  drops the last path segment.
- `VMS_API_KEY` — the API key a build ships with, also only a default. Never set it in
  this file: it is a credential and this file is in git. Pass `-PVMS_API_KEY=…`, or keep it
  in `%USERPROFILE%\.gradle\gradle.properties`.

## Tablets do **not** need registering with ICP for this app

This section previously said the opposite, and it was wrong. Checked against ICP's own
documents:

- `registerDevice(encodedUserId, encodedPassword, deviceReferenceId)` "registers a device with
  the **Validation Gateway** (VG) against the Service Provider (SP) licence" — *Developer Guide
  Java & Android v1.30 §5.1.8*. It is a VG operation, and it needs SP credentials that ICP
  issues with VG pre-production access, over a VPN (*Overview v1.5 §4.1.3*).
- Offline mode has no such step. *Overview §4.2*: a Service Provider "must download the Toolkit
  along with the offline configuration files and integrate them with the business
  applications", and "the Toolkit solely permits the reading of open public data from the
  card". Nothing about registration.
- `readPublicData` lists no registration precondition (*§5.2.11*).

This deployment holds an **offline** bundle and sets `read_publicdata_offline = true`, so a
read never reaches the VG and there is nothing to register against. The reception PCs already
read cards on this bundle without any device having been registered, which is the same fact
arrived at from the other end.

Two details the old text also had backwards, worth keeping straight for whenever the online
licence does arrive:

- **`getDeviceId()` is for *de*registration**, not registration — "required to deregister a
  device through the ICP Web Service REST API" (*§5.1.9*). It is not an identifier to send ICP
  in order to register.
- **`deviceReferenceId` is generated by us**, not by ICP or the toolkit: "Unique device ID
  generated by the SP for internal device management" (*§5.1.8*).

Registration becomes real work the day ICP activates the production licence, and at that point
it needs the SP user ID and password, `prepareRequest` first, and VG connectivity over MPLS —
see `docs/icp-signed-response-request.md`.

## The reader ICP lists for Android is the ACR38u, not the ACR39U

*Overview v1.5*, Annexure-1, is the compatible-device table. Its only ACS entry for **Android**
is the **ACR38u** (Smartcard OTG, contact). The ACR39U appears nowhere in it, and the desks
have ACR39Us.

That is not proof it will fail — the same annexure says the table reflects "the data accessible
to ICP at the document's release", the two are the same family, and `device_filter.xml` claims
the ACS vendor without a product ID so the tablet will attach either. But ICP's own advice in
that table is to "conduct tests with a sample of the specific device using the Toolkit" before
buying in bulk, and no ACR39U has yet been tried on Android here. Try one on a tablet before
ordering anything, and if it will not attach, an ACR38u is the reader ICP has actually tested.

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
