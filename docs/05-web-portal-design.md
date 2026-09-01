# Web Portal Design

React 18 + TypeScript SPA against `DI.Vms.Api`. Security and Admin surface (BRD §24).

**No card reading.** The JS toolkit is Java Web Start / local-Windows-service based
(SDK analysis §8); the portal is a management and reporting tool only.

## Navigation (BRD §24)

```
Dashboard ├── Current Visitors      ├── Reports
          ├── Visitor Registration  ├── DI Entities
          ├── Visitor History       ├── Employees / Hosts
          ├── Expected Visitors     ├── Building / Floor / Office
          ├── Emergency             ├── Users & Roles
                                    ├── Configuration
                                    └── Audit Log
```

## Dashboard (BRD §11)

Four counters — Total Visitors, Currently Inside, Checked Out, Expected — above the
live "currently inside" table (visitor, company, visiting, entity, floor, in-time, status).

Live via SignalR, not polling. A security dashboard showing a 30-second-old occupancy
figure is misleading in exactly the situation where it matters.

## Emergency (BRD §12)

Treated as a **first-class screen, not a report** — the BRD is explicit that this is
mandatory. Requirements that follow from how it is actually used:

- Reachable in one click from every screen.
- Everyone on site: visitors, contractors, service providers (and employees once §25
  Phase 3 integrates the employee directory).
- Grouped by floor — that is how a building is swept.
- Printable and offline-exportable to PDF. **During an evacuation the network may be down
  and the assembly point has no wifi.** A dashboard that only works online has failed.
- Available to every authenticated role.

## Other screens

| Screen | Notes | BRD |
|---|---|---|
| Visitor History | Search by name, ID, passport, company, entity, host, date, visit no., floor, status | §13 |
| Visitor Profile | Total visits, last visit, previous visits | §13 |
| Expected Visitors | Today's pre-registrations, convert to check-in | §16 |
| Reports | The §20 set; CSV/XLSX export; Power BI for analytics | §20 |
| Masters | DI entities, hosts, buildings/floors/offices | §5, §6 |
| Users & Roles | Role assignment; `CanViewUnmaskedId` as an explicit toggle | §18 |
| Audit Log | Filter by user, entity, record, date; old/new value diff | §19 |

## Presentation rules

- **ID numbers render masked everywhere by default.** Unmasking is a deliberate per-record
  action, permission-gated, and writes an audit row (§22).
- Exports obey the same masking as the screen — an unmasked spreadsheet leaving the building
  defeats the control entirely.
- Times display in Gulf Standard Time; the API speaks UTC throughout.
- Status uses both colour and text (🟢 Inside / 🔴 Checked Out), never colour alone.
