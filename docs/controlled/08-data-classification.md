# Data Classification Statement

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Data Classification Statement (Controlled, required)

| | |
|---|---|
| Application | Visitor Management System (VMS) |
| Application classification | **Controlled** |
| Highest data classification handled | **Confidential — Personal Data** |
| Data subjects | Visitors to the DIP office; Dubai Investments group staff named as hosts |
| Legal basis | Site security and access control at a commercial premises |
| Statement owner | IT (to be named on `11-it-asset-registration.md`) |

---

## 1. Why the application is Controlled

Section 2 of the policy classifies an application by what it handles, not by its size. VMS
handles **government-issued identity documents belonging to members of the public**, and
**employee directory data**. Either alone would be enough.

There is no reading of the scoring model under which this is Non-Controlled, so no
borderline case is argued here.

## 2. What is held

### 2.1 Visitor personal data — Confidential

Read from the Emirates ID chip, from the card's printed face, or typed at the desk. Stored
in `vms.VisitorEntry`; the full column list is in `03-data-dictionary.md`.

| Class | Fields |
|---|---|
| **Government identifier** | `IdNumber` (Emirates ID, 15 digits), `CardNumber`, `IdType`, `IssueDate`, `ExpiryDate` |
| **Name** | `FullNameEnglish`, `FullNameRaw`, `FullNameArabic`, `TitleEnglish` |
| **Demographic** | `Gender`, `DateOfBirth`, `NationalityEnglish`, `NationalityCode`, `PlaceOfBirthEnglish` |
| **Home address** | `AddressEmirate`, `AddressCity`, `AddressArea`, `AddressStreet`, `AddressBuilding`, `AddressPoBox`, `AddressPhone`, `AddressMobile`, `AddressEmail` |
| **Biometric-adjacent** | `Photo` — the JPEG held on the chip |
| **Contact** | `ContactMobile` — the number given at the desk |
| **Card image** | `vms.VisitorCardImage` — the card as read, rendered and kept with the visit |

The home-address block was **empty on every card this project has tested** except mobile
and email. Nothing in the application assumes any of it is present.

### 2.2 Visit data — Internal

`DiEntityId`, `PersonToVisit`, `PersonToVisitTitle`, `PersonToVisitEmail`,
`PersonToVisitCompany`, `Purpose`, `PurposeOther`, `RecordedAtUtc`, `CaptureMethod`,
`RecordedBy`, `SignedOutAtUtc`, `SignedOutBy`.

Who visited whom and why. Not identity data, but it is a movement record of named staff and
is **Internal** rather than Public.

### 2.3 Staff directory — Internal

`vms.Person`: `DisplayName`, `Title`, `Email`, `CompanyName`, `DirectoryObjectId`. Loaded
from the AD export by a generated script, or read live from Entra ID.

**Deliberately never shipped to a device.** `/api/people` searches server-side and returns
at most 12 matches. The whole staff directory sitting on a tablet at a reception desk is
the thing this design refuses.

### 2.4 Credentials and secrets — Restricted

| | Where it belongs |
|---|---|
| SQL connection string | Azure app settings, or `appsettings.Production.json` (gitignored) |
| Entra client secret | Azure app settings |
| Blob storage AccountKey | Azure app settings |
| Tablet API keys (3) | Azure app settings server-side; `%USERPROFILE%\.gradle\gradle.properties` at build time |
| Tablet settings PIN | On the tablet only, as a PBKDF2-HMAC-SHA256 hash. Not recoverable |

**No credential belongs in `appsettings.json` or `appsettings.Development.json` — both are
committed.**

## 3. Sensitive data the system does *not* hold

Stated so that a future reader does not have to infer it:

- No payment data, no card numbers in the financial sense
- No health data
- No biometric template — the chip's photograph is stored, the fingerprint data on the card
  is never requested
- No cardholder signature is read any more. `CardSignature` remains as a column so rows
  written by earlier builds stay readable; both the desk reader and the agent pass `false`
  for it now
- No passwords. Staff authenticate to Entra ID, which VMS never sees a password for

## 4. Handling rules

| Rule | State today |
|---|---|
| In transit | **Met.** HTTPS everywhere. HSTS, CSP, `SameSite=Strict`, `Secure` cookies |
| At rest, database | **Partly.** SQL Server encryption at rest where the instance has it; **no column-level encryption on `IdNumber`** |
| At rest, blob | **Met.** Microsoft-managed keys |
| At rest, device | **Met.** No visitor data is written to a tablet |
| Access control | **Met.** Entra ID app roles; nothing anonymous. See `09-access-control-matrix.md` |
| Display masking | **Not met.** Emirates ID numbers are shown and exported in full. `Vms.UnmaskedId` exists as a role and nothing enforces it — R-03 |
| Access logging | **Not met.** Nobody records who read the report, exported a CSV, or opened a card image — R-04 |
| Retention | **Not met.** Nothing deletes a visit. There is no retention job and no agreed period — R-05 |
| Export control | **Not met.** CSV export is available to anyone with `CanViewReport` and is not logged — R-04 |

## 5. Retention — a decision, not an omission

**There is no retention period set, and nothing in the system deletes a visit.** Rows
accumulate indefinitely.

This is a business decision that has not been taken, not something the build got wrong.
Three things need deciding together:

1. **How long a visit record is kept.** A site-security log is commonly 12–24 months.
2. **Whether the card image and photograph are kept for the same period** as the text
   fields, or shorter. They are the most sensitive part and the least often needed.
3. **Who approves a deletion run**, given that the report is the only record there is.

Until that is decided, the honest statement is the one above.

> **[DECISION REQUIRED — Business Owner and IT Head]** Retention period for
> `vms.VisitorEntry`, and separately for `vms.VisitorCardImage` and `VisitorEntry.Photo`.

## 6. Disclosure

| To | What | Basis |
|---|---|---|
| ICP Validation Gateway (Federal Authority for Identity and Citizenship) | The card validation request | ICP Service Provider licence. This is the issuing authority validating its own document |

There are **no** other outbound flows. No analytics, no telemetry, no third-party OCR, no
AI inference endpoint. Camera reading is on-device in both the tablet and the browser.

## 7. Development-phase exposure

Recorded because the policy's §9 continuity controls assume the record is complete:

Four credentials — the Entra client secret, the SQL password, the `vmsdi` storage
AccountKey and an API key — were pasted into a development chat session. **All four require
rotation.** The SQL password additionally exists in this repository's git history and
cannot be removed from it; rotation is the only remedy.

Real Emirates ID data appears in development-session screenshots and in a PDF held outside
this repository.

Status: the owner has confirmed the credentials will be changed. Until that is done this
row stands open as risk R-02.
