# ID Card Toolkit — SDK Analysis

Analysis of the SDK bundle committed in `12a2135`, against the BRD.

**What it is:** the UAE ICP / Emirates Identity Authority **ID Card Toolkit (EIDA Toolkit) v3.1.6**.
Android package root `ae.emiratesid.idcard.toolkit`; .NET namespace `AE.EmiratesId.IdCard`.

| Bundle | Platform | Binding | Usable for |
|---|---|---|---|
| `id-card-toolkit-android-sdk-v3.1.6` | Android arm64-v8a + armeabi-v7a | `EIDAToolkit.aar` (native, Java/Kotlin) | Android tablet |
| `id-card-toolkit-windows-sdk-v3.1.6` | Windows x86 + x64 | `IDCardToolkit.dll` (**official .NET binding**) + C, Java | Windows reception PC |
| `id-card-toolkit-ios-sdk-v3.1.6` | iOS arm64, iOS 12+ | Swift + Objective-C `IDCardToolkit.framework` | iPad / iPhone |
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

## 3. Client platform: all three are supported, but each carries a binding cost

> **Corrected 2026-09-02.** An earlier revision of this document stated that no iOS
> toolkit existed, because none was present in the first SDK commit. The iOS SDK has
> since been added to the repository. The conclusion drawn from its absence — that iOS
> could not be a Phase 1 target — was wrong and is withdrawn.

BRD §23 recommends ".NET MAUI, one application supporting Android and iOS". That is now
achievable, at a cost worth stating plainly.

| Target | Toolkit form | What MAUI would need |
|---|---|---|
| Android | `EIDAToolkit.aar`, Java/Kotlin | A .NET for Android **binding library** over the AAR, plus the chosen hardware plugin and its native `.so` for both ABIs |
| iOS | `IDCardToolkit.framework`, Swift/ObjC | A .NET for iOS **binding project** over the framework, plus the chosen plugin framework |
| Windows | `IDCardToolkit.dll` | Nothing — it is already an official .NET assembly |

So the choice is:

- **MAUI, one codebase, two binding layers.** Legitimate, and it satisfies BRD §23. The
  binding work sits on the critical path of the highest-risk feature, and binding
  generators handle Objective-C frameworks better than Swift ones — check whether the
  Objective-C variant is usable before committing.
- **Native per platform.** No binding layer over the riskiest code; two codebases.
- **Windows desk client.** No binding layer at all, and PC/SC means any standard reader
  works (see §3a). Lowest risk by a wide margin if reception is a fixed desk.

**Recommendation:** decide this only after the hardware spike in §3a. If a supported
mobile reader is confirmed and tablets are a firm requirement, MAUI is defensible. If
reception is a desk, the Windows client is finished work versus weeks of binding.

## 3a. Reader hardware — the answer differs sharply by platform

| Platform | Supported readers |
|---|---|
| **Windows** | **Any PC/SC-compliant reader.** `plugins/PCSC/` is generic; no vendor list applies. |
| Android | Named plugins only: ACS, Telpo, Feitian, Grabba, GripID, Gen2Wave, IDScreen, Identos, OmaPos, PaySky, Morpho, Artisecure, DsapBioPOS, tech5 — or NFC |
| iOS | Feitian iR301, Grabba, Tactivo — or NFC |

The other Windows plugins (Morpho/Sagem MSO 1350, Secugen, Dermalog, NCR, DsapBioPos) are
fingerprint scanners and specialist terminals, not card readers.

**Confirmed on the development machine, 2026-09-02.** An **ACS ACR39U ICC Reader** is
listed by `SCardListReaders` - the same call the toolkit makes through
`Toolkit.ListReaders()` - and a card returns an ATR. Windows binds it with its own USB
CCID driver, so no vendor driver was needed. `tools/Check-CardReader.ps1` performs this
check.

ACS also appears in the Android plugin list, so the same reader family remains available
if a tablet client is ever revisited.

**Still outstanding:** a real `ReadPublicData` against an Emirates ID via
`quickstart/64/EIDAToolkitApp.exe`. PC/SC visibility proves the hardware chain; it does
not prove the Service Provider licence, device registration or config bundle, which are
the next things that can fail.

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

## 8. The browser path is a local agent over WebSocket — and it is viable

> **Corrected 2026-09-02.** An earlier revision dismissed this path as dead Java Web Start.
> JNLP is only the legacy fallback; the modern samples use a local agent, and the
> conclusion drawn from the JNLP reading — that the portal must not read cards — is
> withdrawn.

`samples/web/modern/` and `samples/web/console/` talk to a **local toolkit agent** over a
WebSocket. `health-probe.js` probes `http://127.0.0.1:9006/health` and derives the socket
options from the reply; the agent is installed by
`installer/ICAToolkitService/{32,64}/ICAToolkitService.msi`. (`toolkitagent.emiratesid.ae`
resolves to loopback, so the agent can present a real TLS certificate for a local socket.)

`eidatoolkit.js` exposes the full surface this project needs — `initialize`,
`listReaders`, `getReaderWithEmiratesId`, `readPublicData`, `isCardGenuine`,
`checkCardStatus`, `registerDevice`, `getDeviceId`, `parseMRZ`, `getLicenseExpiryDate` —
as callback-style calls over that socket. `samples/web/newica/*.jnlp` is the legacy path
and should be ignored.

**Consequence: a Windows reception desk needs no native client at all.** The browser
portal, plus the agent installed locally and a PC/SC reader attached, covers the whole of
BRD §24's registration flow. That removes MAUI, WPF and both binding layers from Phase 1.

The constraint is that the machine must be **Windows with the agent installed** — this is
not a route to reading cards from an arbitrary tablet. Android and iOS still need their
native toolkits. So the honest framing is: the portal is a management surface *and*, on a
properly provisioned Windows desk, a registration client.

---

## Impact on the BRD

| BRD says | SDK reality | Action |
|---|---|---|
| §21 mechanism left open | Chip read over contact/NFC | Settled — specify reader hardware |
| §23 MAUI for Android + iOS | Frameworks exist for both; each needs a binding layer | Viable; decide after the hardware spike |
| §3 "tablet camera/scanner will read the ID" | Camera cannot read the chip | Reader hardware required, not a camera |
| §3 extract Name/ID/Expiry/Nationality/DOB/Photo | All available from `ReadPublicData` | Confirmed feasible |
| §23 web portal | JS SDK talks to a local agent over WebSocket | Portal can also be the Windows registration client |
| (not mentioned) | SP licence + per-device registration + expiries | Add to scope, procurement and monitoring |
| (not mentioned) | `IsCardGenuine`, `CheckCardStatus` | Recommend adding to Phase 1 |
| §22 privacy | Chip read avoids storing document images | Reinforces §3's minimal-data recommendation |

## Open items to confirm with ICP / the vendor

1. ~~Does an iOS ID Card Toolkit exist?~~ **Resolved: yes**, `id-card-toolkit-ios-sdk-v3.1.6`.
2. Which **reader hardware** is in use? On Windows any PC/SC reader qualifies; on mobile it
   must match a shipped plugin. Run the §3a spike to confirm.
3. Does DI hold a current **Service Provider licence**? What are its expiry and renewal terms?
4. **Production** config bundle and production `vg_url`.
5. Is the VG reachable from the DIP network, or is firewall/allowlisting needed?
6. Permitted **retention** of chip-sourced data under ICP terms — this may be stricter than DI's own policy, and BRD §22's retention period must respect whichever is tighter.
