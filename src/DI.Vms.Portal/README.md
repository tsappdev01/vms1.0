# DI.Vms.Portal

Security and Admin portal, Blazor Server (.NET 8).

## Why Blazor rather than the React portal

It consumes `DI.Vms.Application.Contracts` **directly**. There is no second set of
TypeScript types to keep in step, so an API contract change is a compile error here
rather than a runtime surprise. That is the main reason for the move; the React portal in
`src/DI.Vms.Web` still exists and still works, and should be removed once this reaches
parity — not before.

## Run

```
dotnet run --project src/DI.Vms.Api        # terminal 1
dotnet run --project src/DI.Vms.Portal     # terminal 2
```

Portal on `https://localhost:7002`, API on `https://localhost:7001` (`Api:BaseUrl`).

## Pages

| Route | Screen |
|---|---|
| `/register` | **New Visitor** — read the card, record the visit, sign, check in |
| `/checkout` | **Check Out** |
| `/` | Dashboard — KPI tiles, currently inside, visitors by entity |
| `/current` | Current Visitors |
| `/expected` | Expected Visitors |
| `/emergency` | **Evacuation list**, grouped by floor, printable |
| `/history` | Visitor History search |
| `/reports` | All 11 BRD §20 reports, with CSV export |
| `/audit` | Audit Log |
| `/entities`, `/employees`, `/users` | Master data (read-only) |

## Conventions carried over

- **ID numbers render masked.** Unmasking is permission-gated, goes through a dedicated
  endpoint, and is audited server-side. A 403 is the control working, not a fault.
- **Status is a coloured dot plus a text label**, never colour alone. Checked-out is grey,
  not the BRD's red: a completed checkout is not a fault, and red is reserved for
  verification failure and expired ID.
- **The API speaks UTC; the portal displays Gulf Standard Time** (`Services/Gst.cs`).
- **Emergency is a screen, not a report** — unpaged, grouped by floor, printable,
  reachable by every role.
- A failed call renders `ApiError`, never an empty table. On an occupancy screen "no rows"
  and "the API is down" must not look the same.

## Development authentication

The role selector in the top bar sets the `X-Dev-Role` header the API's development
authentication reads, so the BRD §18 matrix can be exercised without four accounts. It is
held in a **scoped** `UserContext`, not on the API client — `AddHttpClient` registers its
typed client as transient, so state kept there would be discarded between calls.

Both the selector and the header disappear when Entra ID is wired up.

## Reading a card

Blazor Server runs on the server, and `DI.Vms.CardBridge` listens on the **reception
machine's** loopback — so the server cannot reach it. The browser can, because the browser
*is* on the reception machine. The reader calls therefore go out through JS interop
(`wwwroot/app.js` → `window.vms.cardReader`) and the result crosses the circuit.

That is why `CardReaderClient` uses `IJSRuntime` rather than `HttpClient`, and it is the
reason a Blazor Hybrid desktop app is not required to make this work.

To use a real card:

1. Run `DI.Vms.CardBridge.exe` on the reception machine.
2. Open the portal **in a browser on that same machine**.
3. New Visitor → Read Emirates ID.

Without the bridge the page says so plainly and offers manual entry, rather than appearing
to work.

## Not yet built

Master-data create and edit are visibly disabled rather than absent, so the remaining
scope is legible in the UI. Host notification, the visitor badge and pre-registration are
Phase 2.
