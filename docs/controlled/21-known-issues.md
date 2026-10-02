# Known Issues Log

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §4.6 — named in the Standard Documentation Prompt for Controlled
applications, alongside the twenty documents in §6.

Things that are **wrong, incomplete or surprising**, written down so that whoever meets
them next does not have to find out the hard way. `10-risk-assessment.md` is the register
with ratings and treatments; this is the working list, including items too small to be
risks.

**Status** — `Open` · `Open, with a workaround` · `By design` · `Closed`

---

## Card reading

### K-01 · Toolkit error 233 on one tablet — `Open`

One tablet returns `ETSTATUS_SERVER_RESPONSE_ERROR` (233) on every read. Another tablet on
the same network reads normally.

**Established:** the toolkit reaches ICP's Validation Gateway during `readPublicData` and
the failing tablet cannot. **Not** a build regression — the previous APK fails on the same
tablet.

**Unresolved:** why that tablet and not the other.

**Workaround:** use another tablet, or type the details in. `read_publicdata_offline = true`
is set, and Settings has "Read cards without ICP's gateway" (default off).

**Needed to take it further:** the server's `Toolkit:Agent:ToolkitConfig` value, and a
comparison of the reader-panel line — `<reader name> · Toolkit <version> · licence to
<date>` — between a working tablet and the failing one. Both are information, not code.

Risk R-08. `16-admin-guide.md` §8.

### K-02 · Tablet card reads are not signed — `Open`

ICP's offline bundle returns unsigned responses. A desk read produces signed XML the server
verifies; a tablet read does not. Open with ICP — `../icp-signed-response-request.md`.

Risk R-01.

### K-03 · `read_publicdata_offline` was removed on a bad search — `Closed`

Version 1.4.0 removed the flag because a search found no reference to it. The search looked
in `libc++_shared.so` (the C++ runtime) and `classes.jar` (compressed, so `strings` finds
nothing in it). The binary that ships it is `libEIDAToolkitJNIWrapper.so`, which has seven
matches. Restored in 1.4.1 (`062111a`).

**Search the binary that ships.** Recorded in the code comment as well as here.

### K-04 · The camera reads the card number and nothing else — `Closed`

ML Kit returns text blocks in **block order, not reading order**, so the parser was being
handed the card's lines shuffled. Fixed by sorting lines by `boundingBox.top`, then `left`
(`bb24fb1`).

### K-05 · The cardholder signature image never rendered — `By design`

The chip's signature format did not render in a browser and the slot showed a broken image,
so the read stopped asking for it. Both the desk reader and the agent now pass `false`.

`VisitorEntry.CardSignature` remains **as a column** so rows written by earlier builds stay
readable, and so removing it does not need a migration. To bring it back, ask for it again
in `CardReaderService` and `card-agent.js`.

### K-06 · The chip's home address is empty — `By design`

Every field of the home-address block except mobile and email was empty on the cards this
project has tested. Nothing in the application assumes any of it is present, and the
columns exist because the chip defines them, not because they are populated.

### K-07 · A UAE Pass digital ID can never be verified — `By design`

It carries a QR code where a machine-readable zone would be, and that code is a verification
token for ICP's own service rather than the card's contents. The printed face is read and
offered as a **suggestion**; `complete` stays false and the visit is recorded as `Manual`.

## Input and screens

### K-08 · Typing `7841980` produced `784-91` on the tablet — `Closed`

The field reformatted its own value, so a string that grew in the middle left the caret
before the character just typed. There is no arrangement of that approach that works,
because the caret is not the value's to decide. Fixed by holding digits only and painting
the separators over the top with a `VisualTransformation` and an `OffsetMapping`
(`b2597cd`).

**Web was unaffected** — a browser puts the caret at the end. The bug existed only on the
tablet, which is why it was reported as "its only in tbalet not in web".

### K-09 · A missing field looked like a failed card read — `Closed`

Every 400 on a card visit carried the same ProblemDetails title, so the tablet treated a
missing mobile number as a rejected read and threw the officer back to Insert Card. Now
there are two titles — `The visit was not saved` and `The card read was not accepted` —
and the client matches on them (`0f9d0e4`).

### K-10 · The three validation points disagreed — `Closed`

The web required a mobile number and the tablet called it optional. The web warned that an
ID number was twelve digits and saved it anyway. The server asked only whether the field
contained a digit, so `7` was a valid Emirates ID. One definition now, in
`Services/VisitorFields.cs`, used by all three (`37e9378`).

### K-11 · Emirates ID numbers are shown in full — `Open`

Everywhere: the confirmation, the report, the CSV, the sign-out list. `Vms.UnmaskedId`
exists as a role and nothing calls `CanViewUnmaskedId`. Risk R-03.

### K-12 · The report is 19 columns wide — `Open, with a workaround`

More than fits a 1440px screen. It scrolls sideways. A column chooser would be the fix.

## Data and records

### K-13 · Nothing deletes a visit — `Open`

No retention period is set and no deletion job exists. Rows, card images and chip
photographs accumulate indefinitely. A business decision, not a build defect. Risk R-05,
`08-data-classification.md` §5.

### K-14 · No record of who read or exported the report — `Open`

The one place personal data leaves in bulk is the one place with no trail. Risk R-04.

### K-15 · A visit stays open until somebody closes it — `By design`

Nothing signs a visitor out automatically. Somebody who leaves without telling reception
stays on the "still in the building" list.

This is deliberate and should stay that way unless it is replaced by something visibly
different: a system that closed entries at midnight would produce an evacuation list that
was tidy and wrong. Risk R-11.

### K-16 · A visit cannot be edited or deleted — `By design`

There is no update path and no delete path through the application. It is why there is no
audit table — a row that cannot be changed is its own record. It is also why a mistake at
the desk stays in the record, which is the trade accepted.

The argument holds exactly as far as the application. Anyone with database credentials can
change anything and nothing in VMS would show it (R-10).

### K-17 · `RecordedBy` is null on early rows — `By design`

Null means "recorded when the system had no idea who anyone was", not "unknown user". Rows
written with `Authentication:Enabled = false` carry the string `(not signed in)` instead.
The two are deliberately distinguishable and neither is backfilled into something that
reads like a name.

### K-18 · Documents 01–07 describe a system that does not exist — `By design`

`docs/01-architecture.md` through `docs/07-implementation-plan.md` describe the
Android/React/REST architecture abandoned on 3 September 2026. They are kept for BRD
requirement tracing and the section references, and `docs/README.md` marks them superseded.

**Do not read them as a description of this system.** `docs/controlled/` is.

## Build, deployment and process

### K-19 · Four development credentials are not yet rotated — `Open`

The Entra client secret, the SQL password, the `vmsdi` storage AccountKey and an API key
were pasted into a development chat session. The SQL password is additionally in this
repository's git history permanently, where rotation is the only remedy.

Risk R-02, and the first row of the technical gate on `14-go-live-approval.md`.

### K-20 · Nothing built the .NET projects in CI until 2 October 2026 — `Closed`

Only the APK was built. A change that did not compile could be pushed and discovered on a
deployment. `.github/workflows/dotnet.yml` (`a6be90f`).

### K-21 · The secret scan warned on a clean tree — `Closed`

`if grep … | tee …` tests `tee`'s exit status, which is always success. The step warned on
every run whether it had found anything or not, which is how a check teaches people to
ignore it. Fixed by capturing into a variable with `|| true` (`5788a45`).

### K-22 · No Semgrep SAST step — `Open`

Required by policy §7. Risk R-07.

### K-23 · Input validation is not FluentValidation — `Closed`

Required by policy §4.4. The validation was present, server-side and unified, but not in
the named library.

`Services/VisitValidators.cs` now declares every refusal `POST /api/visits` can return, as
FluentValidation rules resolved from DI into the endpoint in both hosts. It is a wrapper:
the judgements stay in `Services/VisitorFields.cs`, because a second opinion about what a
valid Emirates ID number is — living in a validator — is exactly the drift that put three
different answers in three places before `VisitorFields` existed.

The rules are in **three rule sets** rather than one pass, because order is part of the
behaviour: a request with no mobile number and an unverifiable card read must still be told
about the card, since the tablet decides whether to send the officer back to it by the
refusal's title. Deviation D3 closed.

### K-24 · The repository is GitHub, not Azure DevOps — `Open`

Required by policy §4.4 and §7. Equivalent controls are available and in place. Deviation
D1 — needs written Policy Owner approval.

### K-25 · A Linux Web App answers 503 with the Windows build — `By design`

`DI.Vms.Blazor` is `net8.0-windows` because it references ICP's `IDCardToolkit.dll`. The
Web App's OS is fixed when the plan is created and cannot be changed afterwards. Build with
`-p:VmsAgentOnly=true` and set `Toolkit__Mode=Agent` for Linux. `../azure-deployment.md`.

### K-27 · A win-x64 package on the Linux Web App looks like a database fault — `Closed`

On 2 October 2026 the Azure Web App was given the `-r win-x64` build. It did not fail
cleanly. It started, reached the database, and died:

```
System.NullReferenceException
   at Microsoft.Data.Common.ADP.LocalMachineRegistryValue(String subkey, String queryvalue)
   at Microsoft.Data.SqlClient.TdsParserStaticMethods.AliasRegistryLookup(String& host, ...)
```

A `-r win-x64` publish ships the **Windows** build of `Microsoft.Data.SqlClient`, which
looks up SQL Server aliases in the Windows registry before connecting. There is no registry
on Linux, so the lookup returns null and the client dereferences it. Five retries, exit code
134, crash loop.

**Nothing in that message says "wrong platform" or "wrong RID".** It reads as a connectivity
problem, and the startup retry treated it as one — the connection string was never at fault.
Visits 196–203 were recorded normally right up to the deployment and none were lost.

Two causes, both now fixed:

1. `docs/azure-deployment.md` said the Web App was a **Windows** plan. It is Linux — the
   container log says `A P P S E R V I C E   O N   L I N U X`. The plan was created or
   recreated as Linux and the table was never updated, so the wrong build was handed over on
   the strength of a document instead of the running system. Corrected, with this failure
   named in it.
2. The CI artifacts were called `vms-web-reception-win-x64` and `vms-web-portable`, which
   says what each *is* and not where each *goes*. They now say where they go.

**The running system is the authority on which host this is, not any document here.**

### K-26 · Four view-model functions were deleted by an edit — `Closed`

`testServer`, `saveServer`, `resetServer` and `applySettings` were removed when a range of
`VisitorViewModel.kt` was rewritten by markers. CI caught it; restored in `c1cf899`, and a
member-diff check added afterwards.

---

## Summary

| Status | Count |
|---|:-:|
| Open | 8 |
| Open, with a workaround | 1 |
| By design | 8 |
| Closed | 10 |

The eight open items are K-01, K-02, K-11, K-13, K-14, K-19, K-22, K-24. Of those,
**K-19 is the one to act on before anything else**, and it is not a code change.
