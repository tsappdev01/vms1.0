# DI.Vms.Web

Security and Admin web portal for the Dubai Investments Visitor Management System.
Design: [`docs/05-web-portal-design.md`](../../docs/05-web-portal-design.md).

React 18 + TypeScript + Vite. No card reading — see `docs/00-sdk-analysis.md` §8.

## Run

```bash
npm install
npm run dev          # http://localhost:5173
```

Fixtures are the default - no `.env` needed and no backend required, so a fresh
clone runs immediately. Every screen is served from in-memory data and a banner on
screen says so.

Once `DI.Vms.Api` exists:

```bash
# .env
VITE_USE_MOCK=false
VITE_API_URL=https://localhost:7001
```

`src/api/client.ts` is shape-compatible with `src/api/mock/mockClient.ts`, so
switching source is a configuration change rather than a rewrite.

## Scripts

| Command | Does |
|---|---|
| `npm run dev` | Dev server with HMR |
| `npm run build` | Typecheck (`tsc --noEmit`) then production build |
| `npm run typecheck` | Typecheck only |
| `npm run preview` | Serve the production build |

## Screens

Dashboard, Current Visitors, Expected Visitors, **Emergency**, Visitor History,
Reports, Audit Log, DI Entities, Employees / Hosts, Users & Roles.

## Conventions worth keeping

- **ID numbers render masked.** Unmasking is a per-user permission, goes through a
  dedicated endpoint, and is audited server-side. The portal never receives a raw
  ID number in a list or detail response.
- **Status is a coloured dot plus a text label**, never colour alone.
- **The API speaks UTC; the portal displays Gulf Standard Time.**
- **Emergency is a screen, not a report** — one click from anywhere, unpaged,
  grouped by floor, printable and downloadable, available to every role.
- Charts follow the validated data-visualisation palette: sequential blue for
  magnitude, and the fixed status palette never reused as a series colour.

## Not built yet

Reports execution, master-data create/edit, and real authentication all need
`DI.Vms.Api`. Those controls are visibly disabled rather than absent, so the
remaining scope is legible in the UI.
