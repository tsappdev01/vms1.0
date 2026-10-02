# UAT Test Cases

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — UAT Test Cases (Controlled, required)
Covers §4.5 **Scenario 1 — Functional Validation** and **Scenario 2 — Edge Case and
Boundary Testing**. Scenario 3 (Security and Access) is in `07-security-review-checklist.md`
section E, because it is IT-owned and tested differently.

The policy asks for a minimum of 10 scenarios. There are **34** here, because the edge
cases are where this application has actually broken.

**Run on staging.** Record Pass / Fail and the date. A Fail goes back to the build loop
(§4.5) and the scenario is re-run after the fix.

| Run by | |
|---|---|
| Functional (F) | IT **+ the Digital Champion** — the reception supervisor who knows the real workflow |
| Edge case (E) | IT |

---

## Scenario 1 — Functional validation

The Digital Champion walks through their real daily workflow.

| # | Test | Steps | Expected | Result | Date |
|---|---|---|---|---|---|
| F1 | Check in a visitor from the chip | Insert a real Emirates ID chip-first · Read Card · confirm the fields · pick entity, host, purpose · enter mobile · Save | Fields match the card. Entry saved with a reference number. `Source` reads as a card read | ⬜ | |
| F2 | Card photograph, back of a plastic card | Scan the card · confirm | The machine-readable zone is read; name, number, expiry and nationality appear. Saved as a photographed card | ⬜ | |
| F3 | UAE Pass digital ID on a phone screen | Scan it | The printed face is read as a **suggestion**: name, expiry, nationality, ID number. The officer confirms. Saved as Manual | ⬜ | |
| F4 | Full manual entry | Enter details · type 15 digits with no hyphens · type dates with no slashes | Hyphens and slashes appear **as you type**, and the caret stays after the digit just typed. Saves | ⬜ | |
| F5 | Pick a host from the list | Type three letters of a staff name | Matches appear with title and company. Pick one | ⬜ | |
| F6 | Host not in this entity | Type a name with no match · Search all entities | The wider search finds them | ⬜ | |
| F7 | Host not in the directory at all | Type a name nobody matches and continue | The typed name is accepted and recorded | ⬜ | |
| F8 | Purpose "Other" | Choose Other | "Please give details" appears and is required | ⬜ | |
| F9 | Confirmation before saving | Press Save entry | A dialog shows visitor, ID number, entity, host and purpose before anything is written | ⬜ | |
| F10 | Sign one visitor out | Open Sign Out Visitor · find the visitor · Sign out | They leave the list. "Here for" shows how long they stayed | ⬜ | |
| F11 | Sign a group out together | Tick three · Sign out 3 selected | All three leave the list | ⬜ | |
| F12 | End of day | Sign out everyone | A confirmation is required first. All open visits close | ⬜ | |
| F13 | Read the report | Open Visitor Report · set a date range · Apply | Visits grouped by entity, newest first, with the recorded time in GST | ⬜ | |
| F14 | Export | Export CSV | A file downloads containing the same rows | ⬜ | |
| F15 | Print | Print | A legible printed layout | ⬜ | |
| F16 | Tablet check-in, chip | Repeat F1 on a tablet | The same fields are captured as the desk captures. The mobile number from the card is prefilled | ⬜ | |
| F17 | Tablet check-in, camera | Repeat F2 on a tablet | Same result | ⬜ | |
| F18 | Tablet settings are protected | Open Settings on the tablet | A PIN is required | ⬜ | |

**F16 is a parity test and it matters.** The requirement is that whatever the web extracts,
the tablet extracts — all 25 card fields, including the mobile number from the chip's
home-address block.

## Scenario 2 — Edge case and boundary testing

| # | Test | Input / condition | Expected | Result | Date |
|---|---|---|---|---|---|
| E1 | ID number one digit short | `78419794076928` (14 digits) | **Refused.** "An Emirates ID number is 15 digits. That is 14." | ⬜ | |
| E2 | ID number with a wrong check digit | `784197940769281` | **Refused.** "That is fifteen digits but not a valid Emirates ID number — check it against the card." | ⬜ | |
| E3 | ID number with the wrong prefix | `785197940769280` | **Refused.** "An Emirates ID number begins 784." | ⬜ | |
| E4 | ID number typed with hyphens | `784-1979-4076928-0` | Accepted; stored as digits | ⬜ | |
| E5 | Backspace over a hyphen | Type 7 digits, press Backspace twice | Two digits are removed. The field does not get stuck on a separator that reappears | ⬜ | |
| E6 | Caret position while typing | Type `7841980` without pausing | Reads `784-1980`. **Not** `784-91`. Test on **a tablet as well as the web** — this failed only on the tablet | ⬜ | |
| E7 | Mobile number missing | Leave Mobile empty · Save | **Refused.** Mobile is required on the web, on the tablet and at the API | ⬜ | |
| E8 | Mobile with letters | `call me` | Refused with "A telephone number is digits…" | ⬜ | |
| E9 | UAE mobile a digit short | `05512345` | Warned — "A UAE mobile is nine digits after the country code". **Warned, not refused** | ⬜ | |
| E10 | Foreign mobile | `+44 7700 900123` | Accepted without complaint | ⬜ | |
| E11 | Landline | `04 812 3456` | Accepted without complaint | ⬜ | |
| E12 | Expiry date missing | Leave Card Expiry empty · Save | **Refused.** Required on all three paths | ⬜ | |
| E13 | Expired card | Expiry in the past | **Saved, with a warning naming the expiry date.** Reception has somebody standing in front of them; a desk that will not proceed writes the visit on paper | ⬜ | |
| E14 | Unparseable date | `twenty-fifth of August` | Warned that the report will not be able to read it. Not refused | ⬜ | |
| E15 | Card expiring **today** | Expiry = today's date in Dubai | **Not** reported as expired. Tests the UTC+4 offset — a UTC comparison calls it expired for the first four hours of the working day | ⬜ | |
| E16 | Same visitor twice in one day | Check the same card in twice | Two visits recorded against one ID number. Both appear in the report | ⬜ | |
| E17 | Name longer than the column | A 320-character name | Clamped to 300, saved, and the clamp is not silent | ⬜ | |
| E18 | Apostrophe in a host name | `O'Brien` | Stored and displayed correctly | ⬜ | |
| E19 | Arabic name | A card with an Arabic name | Stored and displayed right-to-left without corruption | ⬜ | |
| E20 | Chip will not read | Present a damaged or foreign card three times | After three failures manual entry is offered. The desk is never stuck | ⬜ | |
| E21 | No reader attached | Run on a machine with no reader | The screen says so once, plainly, and offers manual entry. **No Read Card button that cannot work** | ⬜ | |
| E22 | ICP gateway unreachable | Disconnect the desk from the internet, read a card | Either the read succeeds offline, or it fails **naming the toolkit error code** rather than saying "failed" | ⬜ | |
| E23 | Database unreachable | Stop SQL Server, open any screen | A clear failure, not a blank page. `/health` reports it | ⬜ | |
| E24 | Server unreachable from the tablet | Turn the tablet's wifi off, try to save | A clear message. The entry is not silently lost | ⬜ | |
| E25 | Wrong tablet key | Change the key in Settings to a wrong value · Test | The test fails with a message that says the key was refused, not a generic error | ⬜ | |
| E26 | Two desks, one visitor | Check the same visitor in from two tablets at once | Two visits. Neither errors. Each records which desk saved it | ⬜ | |
| E27 | Sign out a visitor already signed out | Two supervisors sign out the same person | No error; the second is a no-op. The sign-out time is the first one | ⬜ | |
| E28 | Report over an empty range | A date range with no visits | "No entries", not an error or an empty page | ⬜ | |
| E29 | Report over a large range | 12 months | Returns within a reasonable time. Grouping and ordering correct | ⬜ | |
| E30 | Camera finds nothing | Point the scanner at a blank wall for 15 seconds | A message saying what it is looking for. **Not silence** | ⬜ | |
| E31 | Photograph of only part of the card | Half the zone visible | Refused with a reason. No half-read identity is offered | ⬜ | |
| E32 | Entity list changed while a form is open | Run a `db/` script adding an entity, then save | The save succeeds. A new entity appears after the next load, with no rebuild | ⬜ | |
| E33 | Startup with a missing migration | Start against a database missing a required column | **Startup refuses and names the script.** Not `Invalid column name` on the first query at a desk | ⬜ | |
| E34 | Restart mid-check-in | Restart the app with a card read on screen | The circuit reconnects or the page restarts cleanly. No half-saved visit | ⬜ | |

---

## Regression

After any fix, re-run: every scenario that failed, plus **F1, F4, F16, E2, E6, E7, E12** as
the standing regression set. Those seven cover the three capture paths, the typing
behaviour that has broken twice, and the three rules that block a save.

## Result

| | |
|---|---|
| Scenarios run | ____ of 34 |
| Passed | ____ |
| Failed | ____ |
| Re-run after fix | ____ |

| | Name | Signature | Date |
|---|---|---|---|
| IT | | | |
| Digital Champion | | | |
