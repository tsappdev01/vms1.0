# ID Card Toolkit — SDK Analysis

Analysis of the SDK bundle committed in `12a2135`, against the BRD.

**What it is:** the UAE ICP / Emirates Identity Authority **ID Card Toolkit (EIDA Toolkit) v3.1.6**.
Android package root `ae.emiratesid.idcard.toolkit`; .NET namespace `AE.EmiratesId.IdCard`.

| Bundle | Platform | Binding | Usable for |
|---|---|---|---|
| `id-card-toolkit-android-sdk-v3.1.6` | Android arm64-v8a + armeabi-v7a | `EIDAToolkit.aar` (native, Java/Kotlin) | Android tablet |
| `id-card-toolkit-windows-sdk-v3.1.6` | Windows x86 + x64 | `IDCardToolkit.dll` (**official .NET binding**) + C, Java | Windows reception PC |
| `id-card-toolkit-windows-web-javascript-sdk-v3.1.6` | Windows browser | `eidatoolkit.js` + JNLP/Java Web Start + local Windows service | Browser at a Windows desk |
| `IDCARDOFFLINE_config_2026-07-29` | — | 5 encrypted config blobs | Toolkit configuration |

---

## 1. The BRD's biggest open question is now answered

BRD §21 deliberately left the reading mechanism open — "OCR, MRZ scanning, NFC, barcode/QR, ID-card reader, third-party SDK". The toolkit settles it:

**Emirates ID data is read from the card's chip, over a smart-card or NFC interface. It is not OCR of the card face.**

The core call is:

```
CardPublicData ReadPublicData(
    string  requestId,
    bool    readNonModifiableData,   // name, ID number, DOB, nationality, sex, expiry
    bool    readModifiableData,
    bool    readPhotography,         // holder portrait
    bool    readSignatureImage,      // holder's signature, as held on the card
    bool    readAddress)             // home and work address
```

Every field BRD §3 asks for — Name, ID Number, Expiry Date, Nationality, Date of Birth, Photograph — comes back from this one call, signed by ICP rather than guessed by an OCR engine. That is a materially stronger basis for a security system than the BRD assumed.

### Consequence: reception needs certified reader hardware

The Android bundle carries **18 hardware plugins**: `ACS`, `Telpo`, `Morpho`, `Feitian`, `Grabba`, `GripID`, `GripIDFab30`, `Gen2Wave`, `Gen2WaveRP70A`, `IDScreen`, `Identos`, `OmaPos`, `PaySky`, `DsapBioPOS`, `ArtisecureSmartCard`, `ArtisecureFingerPrint`, `tech5airsnap`, `NFC`.

A generic iPad or Samsung tablet will not read an Emirates ID. Reception needs either a supported contact reader or a device whose NFC stack is on that list. **Hardware selection is a procurement dependency that must be settled before the tablet app is built.**

## 2. NFC is not tap-and-go

```java
cardReader = ConnectionController.initConnection(tag);
ConnectionController.setNFCParams(cardNumber, dob, expiryDate);
```

`SetNfcAuthenticationParameters(cardNumber, dob, expiryDate)` must be called **before** the chip will release data over NFC. This is BAC-style key derivation: the three values are the access key.

So the NFC flow is necessarily two-stage — obtain card number, date of birth and expiry first (by OCR/scan of the printed MRZ, or by typing them), *then* read the chip. A single tap cannot work.

The contact-reader flow has no such constraint: insert card, `ReadPublicData`, done. **For a fixed reception desk, a contact reader is the simpler and faster design.**

`Toolkit.ParseMRZData(string mrz)` is provided to turn an MRZ string into those fields, so an MRZ scan feeding the NFC unlock is the intended pattern where NFC is required.

## 3. .NET MAUI cannot consume this SDK as-is — and there is no iOS SDK

BRD §23 recommends ".NET MAUI, one application supporting Android and iOS". Both halves have a problem:

- **iOS: there is no iOS toolkit in this bundle.** Android and Windows only. An iOS app cannot read an Emirates ID with what has been supplied. Unless ICP ships an iOS variant, **iOS cannot be a Phase 1 target**.
- **Android via MAUI:** `EIDAToolkit.aar` is a native Android library. MAUI would need a .NET for Android **binding library** wrapping the AAR, plus bindings for whichever hardware plugin is chosen, plus the native `.so` payloads packaged correctly for both ABIs. That is real, ongoing integration work, and it sits on the critical path of the highest-risk feature.

**Recommendation:** build the tablet app as a **native Android (Kotlin)** application for Phase 1. The SDK's sample, documentation and 18 plugins are all Java/Kotlin; going native removes the binding layer from the riskiest part of the system. MAUI remains reasonable for a *later* iOS companion app that does everything **except** ID reading.

If a single cross-platform codebase is a hard requirement, the fallback is MAUI + a hand-written Android binding library — budget for it explicitly, and accept that iOS still cannot scan.

### The overlooked third option: a Windows reception desk

The Windows SDK ships `IDCardToolkit.dll`, an **official, documented .NET binding** (`AE.EmiratesId.IdCard`), with a working WPF sample. If reception is a fixed desk rather than a roaming tablet, a Windows client is the **lowest-integration-risk** path: no binding layer, no AAR, official .NET support, and the same API surface.

The BRD assumes tablets. Worth confirming that assumption is a requirement and not an aesthetic preference — it is the single largest cost driver in the build.

## 4. Device registration and licensing — absent from the BRD

```
Toolkit.GetDeviceId()
Toolkit.RegisterDevice(encodedUserId, encodedPassword, deviceReferenceId)
Toolkit.GetLicenseExpiryDate()
Toolkit.GetConfigCertificateExpiryDate()
```

Every device must be **registered with the ICP Validation Gateway (VG) against a Service Provider (SP) licence** before it can be used. The BRD does not mention this anywhere. It implies:

- DI must hold a valid ICP **Service Provider licence** with SP credentials.
- Each reception device is individually registered and carries a device registration ID.
- **The licence and the config certificate both expire.** Two `Get*ExpiryDate` calls exist precisely so applications can warn ahead of time. If either lapses, reception stops working. This needs a monitored expiry alert, not a calendar reminder.

Replacing a broken tablet is therefore not a like-for-like swap — it needs re-registration. Build that into the operations runbook.

## 5. The supplied configs are QA, not production

```
config_vg_qa    config_tk_qa    config_lv_qa    config_li    config_pg
```

Three of the five are explicitly `_qa`. They are encrypted blobs consumed by the toolkit. **Production configs must be obtained from ICP before go-live**, and the `vg_url` repointed from QA to the production Validation Gateway.

## 6. Online vs offline

Toolkit init from the Android sample:

```
config_directory = <path>
log_directory = <path>
read_publicdata_offline = true
vg_url = <validation gateway URL>
plugin_directory_path = <nativeLibraryDir>
```

`read_publicdata_offline = true` means **reading the card works without network**. But these need the VG online:

| Call | Purpose | Needs VG |
|---|---|---|
| `ReadPublicData` | Identity fields, photo, signature, address | No |
| `IsCardGenuine` | Card is authentic, not a forgery | **Yes** |
| `CheckCardStatus` | Card is not lost/stolen/expired/revoked | **Yes** |
| `AuthenticateBiometricOnServer` | 1:1 fingerprint against ICP | **Yes** |
| `RegisterDevice` | Device enrolment | **Yes** |

This gives a clean degraded mode: if the VG is unreachable, still register the visitor from the chip data, mark the visit `VerificationPending`, and let it reconcile later. Reception never blocks on a network fault — worth designing in deliberately.

## 7. Capabilities worth adopting that the BRD never asked for

The BRD treats ID scanning purely as a data-entry shortcut. The toolkit offers genuine **security** functions that go directly to BRD §26's "identity-verified" principle:

- **`IsCardGenuine(requestId)`** — cryptographic proof the card is real. A VMS that only OCRs a card face can be defeated by a colour printer; this cannot. **Strongly recommend adopting in Phase 1.**
- **`CheckCardStatus(requestId)`** — is the card reported lost, stolen or revoked. High value for a security desk. **Recommend Phase 1.**
- `AuthenticateBiometricOnCard` / `OnServer` — fingerprint 1:1 match proving the bearer is the holder. Appropriate for contractors and high-security areas; Phase 2/3.
- `ReadFamilyBookData`, `AuthenticatePki`, `SignData`, PAdES/XAdES/CAdES document signing — not relevant to visitor management. Ignore.

Note also that the card **already holds the holder's signature image**. That does not replace BRD §7's acknowledgement signature — which is consent captured at the moment of entry, and must still be drawn on the tablet — but it is available for comparison if ever needed.

## 8. The JavaScript SDK is a legacy path — do not build the portal on it

`samples/web/newica/IDCardToolkitService.jnlp` plus `lib/jws/IDCardToolkitService.jar` is **Java Web Start**, which no current browser supports. The modern route is `installer/ICAToolkitService/{32,64}/ICAToolkitService.msi` — a **local Windows service** that `eidatoolkit.js` talks to from the page (there is a `health-probe.js` for exactly that handshake).

Either way, a browser can only read a card on a Windows machine with that service installed and a reader attached. **The web portal should therefore be a pure management/reporting surface with no card-reading responsibility** — which is what BRD §24 already describes. Card reading stays on the registration client.

---

## Impact on the BRD

| BRD says | SDK reality | Action |
|---|---|---|
| §21 mechanism left open | Chip read over contact/NFC | Settled — specify reader hardware |
| §23 MAUI for Android + iOS | Android AAR; **no iOS SDK** | Native Android Phase 1; iOS cannot scan |
| §3 "tablet camera/scanner will read the ID" | Camera cannot read the chip | Reader hardware required, not a camera |
| §3 extract Name/ID/Expiry/Nationality/DOB/Photo | All available from `ReadPublicData` | Confirmed feasible |
| §23 web portal | JS SDK is JNLP-legacy | Portal = management only, no scanning |
| (not mentioned) | SP licence + per-device registration + expiries | Add to scope, procurement and monitoring |
| (not mentioned) | `IsCardGenuine`, `CheckCardStatus` | Recommend adding to Phase 1 |
| §22 privacy | Chip read avoids storing document images | Reinforces §3's minimal-data recommendation |

## Open items to confirm with ICP / the vendor

1. Does an **iOS** ID Card Toolkit exist? This decides whether iOS is ever in scope.
2. Which **reader hardware** is approved, and which of the 18 plugins matches it?
3. Does DI hold a current **Service Provider licence**? What are its expiry and renewal terms?
4. **Production** config bundle and production `vg_url`.
5. Is the VG reachable from the DIP network, or is firewall/allowlisting needed?
6. Permitted **retention** of chip-sourced data under ICP terms — this may be stricter than DI's own policy, and BRD §22's retention period must respect whichever is tighter.
