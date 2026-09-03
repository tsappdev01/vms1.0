# VMS 1.0 — Dubai Investments Visitor Management System

Digital registration and tracking of visitors entering the Dubai Investments office in DIP.

## Status

The application code was **reset on 2026-09-03** to start fresh. The design and analysis
documents in `docs/` were kept, because they record findings that cost real effort to
establish and would otherwise have to be re-derived.

Everything removed is still in git history (up to commit `c98ce08`) if any of it is wanted
back.

## Repository layout

```
docs/                       Design and analysis — start with docs/README.md
id-card-toolkit-*/          ICP ID Card Toolkit v3.1.6 (Android, iOS, Windows, Web JS)
IDCARDOFFLINE_config_*      Toolkit configuration bundle
```

## What is established, and worth not relearning

| | |
|---|---|
| Emirates ID is read **from the chip**, not by OCR | `docs/00-sdk-analysis.md` §1 |
| On Windows, **any PC/SC reader** works — confirmed with an ACS ACR39U | §3a |
| Toolkits exist for Android, iOS and Windows; Windows has an official .NET binding | §3 |
| The browser path is a **local agent over WebSocket**, not dead Java Web Start | §8 |
| Every device needs registering against an ICP **Service Provider licence**, which expires | §4 |
| The licence was initially **PRE-PRODUCTION and rejected** by the gateway; ICP's activated bundle fixed it | §3b |

### Two things a real card read established

**Names arrive comma-delimited with empty positions.** `NAYYAR JAWAID,,,,,ALI KHAN,` is one
person, not seven fields. Joining the non-empty segments gives the usable name.

**The home address was empty on the card tested.** Every address field blank; only mobile
and email came back. Worth confirming against a second card before promising address
capture.

## Scope for the rebuild

Three modules, per the requirement of 2026-09-03:

1. **Visitor Management** — new visitor, read the ID card, fetch public data, photograph
   and home address, select the DI entity and person being visited, save with a date-time
   stamp.
2. **Reports** — Visitor Details by Entity: all information plus the date-time stamp.
3. *(third module to be confirmed)*

Stack: .NET Blazor.

## Database

`UATWEB01`, database `VMS`. **The previous schema is still applied there** — deleting the
scripts did not unapply it. Objects under the `vms` schema from the earlier build remain
until dropped.
