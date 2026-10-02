# API Reference

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — API Documentation (Controlled, required)

Every endpoint the system exposes. Written from `src/DI.Vms.Blazor/Api/VisitsApi.cs` and
`src/DI.Vms.Blazor/Program.cs`; `docs/android-api.md` remains the tablet-builder's guide to
the four endpoints the Android app uses and the one setting that decides whether it can
reach them at all.

---

## Authentication

Every endpoint requires authentication. `options.FallbackPolicy = options.DefaultPolicy`
is set, so there is no anonymous surface — an endpoint added later is protected by default
rather than by whoever remembers the attribute.

Three ways to arrive at `/api`, **and the same authorisation policy decides afterwards**:

| Scheme | Header | Used by |
|---|---|---|
| Entra ID cookie | — (browser session) | the desk screens, via the Blazor circuit |
| API key | `X-Api-Key: <key>` | the Android tablets, one key per tablet |
| JWT bearer | `Authorization: Bearer <token>` | another service, where configured |

The key is *a way of arriving, not a way around the rules*. A tablet presenting a valid key
is still subject to `CanCheckIn` and to every field rule the desk is subject to.

### Rate limiting

On the internet-facing host only: **300 requests per minute per caller IP** across `/api`,
fixed window. Rejections are logged with the caller address and the path. UATWEB01
registers no limiter — it is reachable only from the office network, and hard-coding a
policy name there would take UATWEB01 down to protect Azure.

### Errors

Refusals are RFC 7807 ProblemDetails with HTTP 400 and one of two titles. **The title is
the contract**, not the detail text — the tablet matches on it:

| Title | Means | The client should |
|---|---|---|
| `The visit was not saved` | A field was missing or refused | keep the card read, fix the field |
| `The card read was not accepted` | The read itself was refused — spent request ID, a response that will not verify, no document attached | send the officer back to the card |

Before these were distinguished, every 400 on a card visit sent the officer back to Insert
Card, so a missing mobile number looked like a failed card read.

---

## `GET /api/reference`

Everything reception needs to fill the form, in one call — because a tablet on office wifi
is not a desk on ethernet, and three round trips to draw one screen is three chances to be
halfway through.

**Response** `200 OK`

```json
{
  "entities": [ { "id": 1, "name": "Dubai Investments Park" } ],
  "purposes": ["Meetings","Submission","Delivery","Collection",
               "Visit","Interview","Enquiry","Payments"],
  "otherPurpose": "Other"
}
```

Only active entities, ordered by name. The purpose list is a closed vocabulary held in
`Services/VisitPurposes.cs` — in code rather than in a table, because unlike the entity
list it belongs to the form and not to the group's company structure.

## `GET /api/people`

The host list, searched **server-side**. Deliberately not shipped to the device: it is the
whole staff directory — names, titles, email addresses and employers — and that is not a
thing to leave sitting on a tablet at a reception desk.

| Query | Type | Meaning |
|---|---|---|
| `q` | string | the search term |
| `entityId` | int | narrow to one entity (ignored when the directory is the source) |
| `allEntities` | bool | search every entity |

Two sources, and **the device cannot tell which it got**: Entra ID when the tenant is
configured, otherwise the `vms.Person` table.

- **Directory source** — no entity narrowing, because the directory's `companyName` does
  not use the same names the entity list does, and filtering on a guess at that mapping
  would hide hosts rather than narrow to them. An empty term lists the head of the
  directory so the picker can show something before anything is typed.
- **Table source** — a term of fewer than 2 characters returns empty. Matches anywhere in
  the display name, or as a prefix of the email. At most 12 results, ordered by name.
  A typed `%` is escaped — not injection (EF parameterises the term) but it would otherwise
  match the entire staff list.

**Response** `200 OK` — array of:

```json
{ "id": 0, "displayName": "Khalid Al Mansoori", "title": "Facilities Manager",
  "companyName": "Dubai Investments Park", "directoryObjectId": "a1b2…" }
```

`id` is `0` for a directory hit — it has no row yet. One is written when a visit to that
person is saved. `directoryObjectId` is what identifies the person when `id` is 0.

## `POST /api/reads`

Starts a card read. The device gets an ID it cannot have chosen, spends it once, and cannot
prepare a response before being asked for one.

**Response** `200 OK`

```json
{ "requestId": "…" }
```

## `POST /api/mrz`

What a photographed card says, so the officer can see it before saving.

The client sends **the text its recogniser produced and nothing else**. Every rule about
whether that text is a card — the TD1 check digits, the Luhn digit on the printed number,
the positional repair of OCR-B confusions — lives on the server, where the browser
scanner's rules already live. Two copies of arithmetic that decides whether a visitor
record is real is one copy too many.

**This is a preview, not a commitment.** The save re-reads the same text and decides the
provenance itself, so nothing here is taken on trust later.

**Request**

```json
{ "text": "IDARE784198059198691…\nALI<<NAYYAR<JAWAID…" }
```

**Response** `200 OK`

```json
{
  "ok": true, "problem": null, "complete": true,
  "identity": { "idNumber": "784…", "cardNumber": "…", "fullNameEnglish": "…",
                "nationalityEnglish": "…", "gender": "…",
                "dateOfBirth": "…", "expiryDate": "…" },
  "fromPrint": false, "nameWasCut": false
}
```

| Field | Means |
|---|---|
| `ok` | the text was read as something |
| `complete` | a full machine-readable zone was found — every field carries a check digit |
| `fromPrint` | the fields came from the **printed face** of the card, not a zone. A suggestion the officer must confirm; a visit built from it is recorded as `Manual` |
| `nameWasCut` | the source app truncated the name with an ellipsis (UAE Pass does this). The officer must **finish** it, not check it — a different thing from a misread |

A UAE Pass digital ID has a QR code where a zone would be, so nothing can ever verify it.
`complete` stays `false`, the back of a plastic card still wins if one is presented, and
the name, expiry and nationality printed on the screen are offered rather than ignored.

## `POST /api/visits`

Saves one visit.

**One entry, one provenance.** Send exactly one of `readResponseXml`, `mrzText` or
`manual`. Two of them is a 400.

```json
{
  "requestId": "…",
  "readResponseXml": "<signed XML from the chip>",
  "manual": null,
  "mrzText": null,
  "entityId": 1,
  "personToVisit": "Khalid Al Mansoori",
  "personToVisitId": 42,
  "personToVisitDirectoryId": null,
  "purpose": "Meetings",
  "purposeOther": null,
  "contactMobile": "0551234567"
}
```

**There are deliberately no card fields in this body.** Everything about the visitor comes
out of the signed XML on the server. A name in this body would be a name the server took on
trust from a tablet, and the signature would be decoration.

`manual` carries what reception typed when the chip would not read: `idNumber`,
`cardNumber`, `fullNameEnglish`, `fullNameArabic`, `nationalityEnglish`, `dateOfBirth`,
`expiryDate`, `addressMobile`.

### What the server refuses

Applied to **all three** ways of arriving, by the server, regardless of what any client
already checked:

| Rule | Title |
|---|---|
| An entity is required | `The visit was not saved` |
| A person to visit is required | `The visit was not saved` |
| A purpose is required | `The visit was not saved` |
| Details are required when the purpose is Other | `The visit was not saved` |
| **A card expiry date is required** | `The visit was not saved` |
| **A contact mobile number is required** | `The visit was not saved` |
| A typed ID number must be 15 digits, begin 784, and pass its Luhn check digit | `The visit was not saved` |
| A typed name is required | `The visit was not saved` |
| More than one provenance in the body | `The visit was not saved` |
| The request ID is spent, or the response will not verify, or no document is attached | `The card read was not accepted` |

The typed ID number is **the one typed field that blocks**. It is what a repeat visit is
matched on, so a wrong one does not produce a bad record — it produces a second person. The
officer is holding the card and retyping costs seconds.

Mobile shape and expiry date are **concerns, not problems**: they come back as a `warning`
on the saved visit and never refuse a visitor. Reception has somebody standing in front of
them, and a desk that will not proceed is a desk that writes the visit on paper.

**Response** `200 OK`

```json
{ "id": 114, "recordedAtUtc": "2026-10-02T05:12:44Z",
  "captureMethod": "CardReader", "warning": "That card expired on 25 Aug 2024." }
```

`captureMethod` is one of `CardReader`, `DigitalCard` (a photographed card — the zone is
printed, not signed) or `Manual`.

## `GET /visits/{id}/card`

The rendered card image for one visit. **Requires `CanViewReport`**, not `CanCheckIn`.

Served with its own hard CSP (`default-src 'none'; style-src 'unsafe-inline'; img-src
data:; sandbox`) and `X-Content-Type-Options: nosniff`.

`404` when the row says an image exists and it cannot be produced — a deleted blob, not a
server fault.

## `GET /version`

Plain text. The build identifier.

## `GET /health`

Checks the database. The quickest way to tell "the app is down" from "the network is in the
way" without opening a browser or holding a card. App Service is pointed at this path.

`DI.Vms.Api` serves the same check at both `/health` and `/api/health`.

---

## Not implemented

`docs/03-api-specification.md` describes a REST contract from the superseded
Android/React/REST design — a dashboard, an emergency list, search, history and masters
endpoints. **None of those exist.** That document is kept for BRD requirement tracing and
is not a description of this system. This file is.
