# Change Log / Version History

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Change Log / Version History (Controlled, required)

The policy nominates Azure DevOps as the automatic source for this. The repository is
GitHub (deviation D1 on `14-go-live-approval.md`), so **git is the tamper-evident record**
and this document is the readable summary of it.

```
git log --oneline --reverse          # everything, oldest first
git log --format='%ad %h %s' --date=short -- android/app/build.gradle.kts
```

| | |
|---|---|
| First commit | **2026-09-01** |
| This document covers to | **2026-10-02**, commit `5788a45` |
| Development branch | `claude/new-session-4nlawa` |
| Production releases | **None. The application has not gone live.** |

---

## Android releases

Each is a built, shipped APK. `versionName (versionCode)`, with the commit it was built
from — taken from git, not from memory.

| Version | Code | Date | Commit | What changed |
|---|---|:-:|---|---|
| 1.0.0 | 1 | 2026-09-08 | `02a98fa` | First tablet app |
| 1.1.0 | 2 | 2026-09-30 | `ec8d558` | Settings behind a PIN |
| 1.1.1 | 3 | 2026-09-30 | `37e9378` | Field rules unified with the server's |
| 1.1.2 | 4 | 2026-10-01 | `1ae44a5` | The camera scanner says why it is not detecting anything |
| 1.2.0 | 5 | 2026-10-01 | `debf6e4` | Reads the UAE Pass card's printed face; punctuation added to typed fields |
| 1.2.1 | 6 | 2026-10-01 | `bb24fb1` | Machine-readable zone lines put back in reading order |
| 1.2.2 | 7 | 2026-10-01 | `b2597cd` | **Caret fix** — separators are drawn, not typed into the field |
| 1.2.3 | 8 | 2026-10-01 | `c08a13b` | Mobile number read from the chip on the tablet too |
| 1.3.0 | 9 | 2026-10-01 | `61e6a4b` | All 25 card fields, matching what the web extracts |
| 1.3.1 | 10 | 2026-10-01 | `0f9d0e4` | A refused field no longer looks like a failed card read |
| 1.3.2 | 11 | 2026-10-01 | `19cdfd5` | ICP's error code shown on screen when a read fails |
| 1.4.0 | 12 | 2026-10-01 | `3708c41` | **Withdrawn.** Removed `read_publicdata_offline`, which made it worse than its predecessor |
| 1.4.1 | 13 | 2026-10-01 | `062111a` | `read_publicdata_offline` restored |

> **1.4.0 is on this list rather than quietly absent.** It was built on a search of the
> wrong binary — `libc++_shared.so` is the C++ runtime, and `classes.jar` is compressed so
> `strings` finds nothing in it — and the absence was reported as evidence. The binary that
> ships the flag is `libEIDAToolkitJNIWrapper.so`, which has seven matches. A version
> history that hides its own bad release is not a version history.

## Milestones

### September 2026 — design and first build

| Date | |
|---|---|
| 01 Sep | First commit. SDK analysis, solution design, Phase 1 domain model, re-runnable schema scripts |
| 02 Sep | Reception registration and check-out behind a card-reader port. **ICP licence blocker recorded**, and the unproven degraded mode withdrawn rather than claimed |
| 03 Sep | **Reset.** The Android/React/REST design abandoned and rebuilt as a single Blazor Server application. Documents 01–07 marked superseded that same day |
| 03 Sep | SQL moved to `db/` and kept there. The entity list taken out of the code — reference data is data |
| 03 Sep | Dubai Investments navy and gold; `vms-design.html` becomes the design of record |
| 03 Sep | `db/004_seed_people.sql` **generated** from the AD export rather than hand-written |
| 03 Sep | Confirm-before-saving; purpose of visit with Other taking free text |
| 03 Sep | Deployable to the machine the reader is plugged into; **agent mode** added so the app can run on UATWEB01 with the reader on each desk |
| 04 Sep | First real read through the agent — two faults found. Signature validation made specific: *say what is wrong with the signature, not just that something is* |
| 04 Sep | **An unverified read must not wear the label of a verified one.** Signed-response request raised with ICP |
| 04 Sep | Injection audit recorded with the test to run at the desk; typed values fitted to what can be stored; LIKE stopped reading names as patterns |
| 05 Sep | **Entra ID sign-in**, and `RecordedBy` on every visit |
| 08 Sep | An API for the tablet; the Android reception app |
| 09 Sep | `Microsoft.Identity.Web` patched off an advisory version |
| 17 Sep | Standalone `DI.Vms.Api` for a Linux host |
| 19 Sep | SQL scripts made to work on Azure SQL; `/api` guarded in the Blazor app so both can share one Web App; the public API host hardened |
| 21 Sep | Azure Web App and Azure SQL configured. The card kept as an image with the visit, then moved to Blob Storage. The staff directory read from Entra |
| 21 Sep | **The SQL password taken back out of `appsettings.json`**, and a build refusal added for a credential in a committed file |
| 22 Sep | Development connection string removed from the repository entirely; API keys generated with a cryptographic RNG; startup made to survive a briefly unreachable database |
| 23 Sep | Sign-in on the web screens and a key on the tablet, both at once |
| 29–30 Sep | Authorization code flow instead of implicit; proxy scheme trusted; a user with no role told what is missing instead of getting a 404; the claim map stopped renaming `roles` out from under the role check |
| 30 Sep | **Mobile number for the visitor**, on web and tablet |
| 30 Sep | **Read an Emirates ID from a photograph** — live camera, both sides, zone located rather than assumed, misreads the zone can prove repaired and the ones it cannot refused |
| 30 Sep | **Sign visitors out**, and a printout that is a document rather than a screenshot |
| 30 Sep | Missing indexes created at startup, so there is nothing left to run |
| 30 Sep | APK built in CI; ICP's six missing build requirements applied; the APK's contents checked before publishing |
| 30 Sep | **One key per tablet**, so a visit records which desk saved it |
| 30 Sep | Tablet settings behind a PIN |
| 30 Sep | **Field rules put in one place and made to agree.** Before this the web required a mobile number and the tablet called it optional, and the server asked only whether the ID number contained a digit — so `7` was a valid Emirates ID |

### October 2026 — card reading and security

| Date | |
|---|---|
| 01 Oct | The camera scanner says why it is not detecting anything, rather than nothing |
| 01 Oct | UAE Pass printed face read; hyphens and slashes punctuated into typed fields |
| 01 Oct | **Zone lines put back in reading order.** ML Kit returns blocks in block order, not reading order — the root cause of "it only reads the card number" |
| 01 Oct | **Separators drawn rather than typed into the field.** `7841980` was coming out `784-91` on the tablet because the caret is not the value's to decide |
| 01 Oct | Mobile read from the chip on the tablet; then all 25 fields, matching the web |
| 01 Oct | A refused field stopped looking like a failed card read — two ProblemDetails titles the client can tell apart |
| 01 Oct | ICP's error code put on the screen; `read_publicdata_offline` removed and restored |
| 02 Oct | **Security headers, rate limiting, cookie flags**, and the first CI build of the server at all — until then nothing built the .NET projects in CI, only the APK |
| 02 Oct | Secret-scan step fixed: `if grep … \| tee` tests `tee`'s exit status, which is always success, so it warned on every run whether it found anything or not |
| 02 Oct | **This documentation set**, and a generator for the data dictionary |
| 02 Oct | **Input validation moved to FluentValidation** (policy §4.4). A wrapper over `VisitorFields`, in three rule sets, because order is part of the behaviour |
| 02 Oct | **CI publishes the web build.** Until now nothing produced an installable server package without a .NET SDK and a checkout |
| 02 Oct | **The Linux Web App was given the win-x64 package and crash-looped.** Not a clean failure: the Windows build of Microsoft.Data.SqlClient looks up SQL aliases in the registry, which does not exist on Linux. K-27 |
| 02 Oct | **Builds now name their commit.** Every build reported `v1.0.0` — the SDK default, since nothing set a version — so the screen could not answer which build was running. CI stamps `1.0.<run>-<commit>` |

## What is not here

- **No production release.** Nothing has gone live. `14-go-live-approval.md` is unsigned.
- **No change requests.** §7.2 change control begins at go-live; until then the record is
  the branch and its pull requests.

## After go-live

Every change — of any size — becomes a row in this table, raised as a Change Request work
item linked to the application Feature, on a feature branch, via a pull request reviewed by
IT, through a pipeline run, and past the approval gate.

| Date | Version | Change Request | Change | Approved by |
|---|---|---|---|---|
| | | | | |
