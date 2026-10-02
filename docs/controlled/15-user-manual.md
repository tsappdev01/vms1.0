# User Manual

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — User Manual (Controlled: full)

For reception. If you only read one page, read **"When the card will not read"**.

> Screenshots are marked `[SCREENSHOT: …]` and are to be taken from the production build
> before go-live, at the desk, on the real hardware.

---

## Getting in

Open the VMS address in the browser at the desk, or open the app on the tablet. On the
browser you sign in with your normal Dubai Investments account — the same one as Outlook.
There is no separate password.

If you see **Access denied**, you have signed in but you do not have a VMS role yet. Ask IT.

`[SCREENSHOT: sign-in and the three items in the left-hand menu]`

There are three things in the menu:

| | |
|---|---|
| **New Visitor** | Check somebody in |
| **Sign Out Visitor** | Mark somebody as having left |
| **Visitor Report** | Read the record *(supervisors)* |

Times on every screen are **Gulf Standard Time**.

---

## 1 · Checking a visitor in

Four steps along the top: **Insert Card → Visitor Information → Visit Details → Saved**.

### Step 1 — the card

`[SCREENSHOT: the Insert Card screen, showing "Ready to scan"]`

Ask the visitor for their Emirates ID and put it in the reader **chip first**. Press
**Read Card**.

It takes a few seconds. Hold the card still.

The green **Ready to scan** badge means the reader is working. If it says **Waiting for a
card**, the reader is there but there is no card in it.

### Step 2 — check what was read

`[SCREENSHOT: Visitor Details with the card's fields filled in]`

The card's details appear. **Look at the name and the ID number against the card in your
hand** before you go on. The system read them; you are confirming them.

Two fields must be filled in and the system will not save without them:

| Field | |
|---|---|
| **Card Expiry** | usually read from the card |
| **Mobile** | ask the visitor. The card's own number is filled in where it has one, and you can change it |

If you type an ID number or a date yourself, **type the digits only**. The hyphens in
`784-1980-5919869-1` and the slashes in `25/08/2028` are put in for you as you type.

### Step 3 — the visit

`[SCREENSHOT: Visit Details with the host picker open]`

| | |
|---|---|
| **Entity being visited** | which company |
| **Person to visit** | start typing a name and pick from the list. If they are not listed, press **Search all entities**. If they are still not there, just type the name |
| **Mobile number** | required |
| **Purpose of visit** | choose from the list. Choosing **Other** adds a box for details |

### Step 4 — save

`[SCREENSHOT: the "Save this entry?" confirmation]`

Press **Save entry**. A box shows what is about to be recorded. Check the name and the ID
number one last time, then confirm.

`[SCREENSHOT: the Saved screen with the reference number]`

You get a reference number. **New Visitor** starts the next one.

---

## 2 · When the card will not read

This happens. It is not a fault you have to fix at the desk, and there is always a way
through.

**Read Card again first.** A card that is slightly out of the reader reads on the second
try more often than not.

After three failures the screen offers you the other two ways:

### Scan it with the camera

`[SCREENSHOT: the camera scanner with the frame guide]`

Hold the **back** of the card inside the frame. The camera reads the block of letters and
numbers along the bottom.

It also reads a **UAE Pass digital ID** on a visitor's phone — the name, expiry, nationality
and ID number printed on the screen. For UAE Pass it is reading the *print*, so what it
offers is a **suggestion**: check every field before you save.

If the name ends in `…`, the visitor's phone cut it short. **Finish it yourself** — it is
not wrong, it is incomplete.

If nothing happens for 15 seconds, the camera is not finding a card. Move the card so the
whole bottom block is inside the frame, and make sure the light is not reflecting off it.

### Type it in

Press **Enter details** and type the visitor's details from the card.

The ID number is the one thing the system will refuse. If you get:

> *That is fifteen digits but not a valid Emirates ID number — check it against the card.*

a digit is wrong. The number carries its own check digit, so the system can tell. Read it
off the card again. It is not being awkward: a wrong ID number does not make a bad record,
it makes a **second visitor**.

Everything typed in is marked **Entered manually** on the record, which is correct and
expected.

---

## 3 · Messages you will see

| Message | Means | Do |
|---|---|---|
| *An Emirates ID number is 15 digits. That is 14.* | A digit is missing | Count them off the card |
| *An Emirates ID number begins 784.* | Wrong number entirely | Check you are reading the ID number, not the card number |
| *That is fifteen digits but not a valid Emirates ID number* | One digit is wrong | Read it again |
| *That card expired on 25 Aug 2024.* | The card has expired | **You can still check them in.** The warning is so you know |
| *A UAE mobile is nine digits after the country code — 05x xxx xxxx.* | The number looks short | Check with the visitor. You can save anyway |
| *That has letters in it.* | Something that is not a number is in the mobile field | |
| *That is not a date the report will be able to read.* | The date is in an odd format | Use `25/08/2028` |
| **The visit was not saved** | A field is missing or refused | Fix the field. **The card read is still good** — do not start again |
| **The card read was not accepted** | The read itself failed | Go back and read the card again |
| *Failed to get response from server · toolkit code 233* | The reader could not reach ICP's service | Tell IT. Meanwhile use the camera or type it in |

The two in bold are the important distinction: **"The visit was not saved" does not mean
you have to re-read the card.**

---

## 4 · Signing a visitor out

`[SCREENSHOT: Still in the building]`

**Sign Out Visitor** lists everyone signed in and not yet signed out, with how long they
have been here.

| To | Do |
|---|---|
| Sign one person out | find them and press **Sign out** on their row |
| Sign a group out | tick each, then **Sign out n selected** |
| End of the day | **Sign out everyone**, and confirm |

**Nothing signs a visitor out automatically.** Somebody who leaves without telling you stays
on the list until you take them off. That is deliberate — this list is what an evacuation
would be run from, and a list that tidied itself up overnight would be a list that was tidy
and wrong.

---

## 5 · The report *(supervisors)*

`[SCREENSHOT: Visitor Details by Entity]`

Set **From** and **To**, choose an entity or leave it as All entities, press **Apply**.
Visits are grouped by entity, newest first.

**Export CSV** downloads the same rows as a spreadsheet. **Print** gives a printed layout.

> The report and the export show visitors' **full Emirates ID numbers**. Treat the file the
> way you would treat a photocopy of somebody's ID: do not email it, do not leave it in
> Downloads.

---

## 6 · The tablet

The tablet does the same job as the desk and works the same way. Two differences:

- **Settings is behind a PIN.** That screen holds the key the tablet uses to reach the
  server. If you have forgotten the PIN there is no way to recover it — ask IT, who will
  clear the app's data and set the tablet up again.
- **The tablet has no internet**, only the office network. That is on purpose.

`[SCREENSHOT: the tablet's New Visitor screen]`
`[SCREENSHOT: the tablet's PIN screen]`

---

## 7 · If something is wrong

| What you see | First |
|---|---|
| The page will not load | Check another site. If nothing loads it is the network |
| *Sign-in is off* in the corner | Tell IT. Entries are being recorded without a name against them |
| **Access denied** | You are signed in but have no VMS role. Ask IT |
| The reader panel says a reader is missing | Check the cable. Then tell IT |
| The tablet says it cannot reach the server | Check the tablet is on the office wifi. Then Settings → Test |
| Anything else | `19-incident-response.md` has who to call, and `18-sla-and-support.md` has when |

**You will never be stuck.** If the chip will not read, use the camera. If the camera will
not read, type it in. A visitor standing at the desk is always more important than a
perfect record, and the system is built that way on purpose.
