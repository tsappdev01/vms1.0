# Reception Client Design

Android (Kotlin), the device at the security desk. Screen flow follows BRD §24; the ID
step is dictated by the toolkit (see `00-sdk-analysis.md`).

## Platform

| | |
|---|---|
| Platform | Native Android, Kotlin, minSdk 26 |
| SDK | `EIDAToolkit.aar` via the `:EIDAToolkit` Gradle shim (pulls Gson transitively) |
| Hardware | Certified contact reader, or NFC on a supported device — **to be confirmed** |
| Arch | MVVM, Compose, Room outbox for offline, Hilt |

`EIDAToolkit.aar` needs Gson on the classpath. Use the project-style dependency
(`implementation project(':EIDAToolkit')`) as the sample does; a raw `files(...)` dependency
bypasses transitive resolution and fails dexing.

## Screens (BRD §24)

```
LOGIN ──► SECURITY HOME ──┬── New Visitor ──► IDENTIFY ──► SCAN ID ──► VISITOR INFO
                          │                    ──► VISIT DETAILS ──► HOST ──► SIGNATURE
                          │                    ──► CONFIRM ──► CHECK-IN
                          ├── Expected Visitors
                          ├── Current Visitors
                          ├── Check-Out
                          └── Search Visitor
```

## The scan step

Two flows, decided by hardware. **Contact reader is strongly preferred for a fixed desk.**

**Contact reader:**
```
Toolkit(inProcess=true, config) → getReaderWithEmiratesID() → connect()
  → readPublicData(requestId, true, true, true, true, false)
  → isCardGenuine(requestId) → checkCardStatus(requestId)
```

**NFC** — two-stage, because the chip needs an access key first (SDK analysis §2):
```
scan printed MRZ → parseMRZData() → cardNumber, dob, expiry
  → initConnection(tag) → setNfcAuthenticationParameters(cardNumber, dob, expiry)
  → readPublicData(...)
```

The UI must make this explicit: "Scan the back of the card, then tap it." A user told only
to "tap the card" will fail and retry forever. This is the strongest argument for a contact
reader at a fixed desk.

`readAddress` is `false` — visitor management has no need for a visitor's home address, and
BRD §3 says capture only what is required. `readSignatureImage` is read for verification
only and not persisted; the acknowledgement signature is drawn fresh at §7.

### Verification outcome, shown plainly

`isCardGenuine` and `checkCardStatus` need the VG online. Surface the result as a banner
the officer cannot miss:

| Result | Banner | Behaviour |
|---|---|---|
| Genuine, status valid | 🟢 Verified | Proceed |
| Genuine, card expired | 🟡 ID Expired | Proceed, flagged; supervisor visible |
| Not genuine, or lost/stolen | 🔴 **Verification failed** | Block check-in; escalate to supervisor |
| VG unreachable | ⚪ Pending verification | Proceed, `VerificationPending`, reconcile later |

A red result must not be dismissible by a Security Officer alone — that is the whole point
of having cryptographic verification available.

## Manual fallback

BRD §3 accepts Passport, UAE Driving Licence, GCC ID and other government ID. The toolkit
reads **Emirates ID only**. Everything else is manual entry with `captureMethod = Manual`,
so reports can distinguish chip-verified identities from typed ones. Keep manual entry
available for Emirates ID too — readers fail, and reception cannot stop.

## Offline behaviour

Card reading works offline (`read_publicdata_offline = true`). Registrations queue in a
Room outbox and sync when connectivity returns; visit numbers are allocated server-side at
sync, and the tablet shows a local reference until then.

Host search needs a periodically refreshed local copy of the employee master — searching
hosts is the one thing reception cannot do from memory.

## Device security (BRD §22)

- Device registered with the VG (`RegisterDevice`); registration ID in Android Keystore.
- Client certificate for API mTLS, also in the Keystore.
- Automatic logout on idle; screen pinning / kiosk mode so the tablet cannot leave the app.
- No ID numbers in logs, ever. Log the masked form.
- Managed by Intune (or equivalent) for remote wipe.
- **Warn before the SP licence or config certificate expires** — poll `getLicenseExpiryDate`
  and `getConfigCertificateExpiryDate` at startup and surface a warning well ahead. If either
  lapses silently, reception stops working with no explanation.
