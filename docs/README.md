# VMS Documentation

Design documentation for the Dubai Investments Visitor Management System.
Source requirements: the BRD (26 sections); each document cites the sections it implements.

| Document | Contents |
|---|---|
| [00-sdk-analysis.md](00-sdk-analysis.md) | **Read first.** Analysis of the ICP ID Card Toolkit v3.1.6 and where it contradicts the BRD's assumptions. |
| [01-architecture.md](01-architecture.md) | Components, projects, stack, database target. |
| [02-data-model.md](02-data-model.md) | Entities, lifecycle, indexing, deviations from BRD §17. |
| [03-api-specification.md](03-api-specification.md) | REST contract and authorisation matrix. |
| [04-reception-app-design.md](04-reception-app-design.md) | Android reception client and the ID scan flow. |
| [05-web-portal-design.md](05-web-portal-design.md) | Security/Admin web portal. |
| [06-security-privacy-rbac.md](06-security-privacy-rbac.md) | Roles, ID masking, encryption, audit, retention. |
| [07-implementation-plan.md](07-implementation-plan.md) | Phasing, prerequisites and current status. |

## Headline findings

1. Emirates ID is read **from the card's chip**, not by OCR — certified reader hardware is required.
2. **No iOS toolkit exists**, so the BRD's "one MAUI app for Android and iOS" cannot cover ID reading on iOS.
3. Every device must be **registered with the ICP Validation Gateway** against a Service Provider licence, which **expires**.
4. The supplied configs are **QA**; production configs are needed from ICP.
5. The SDK can prove a card is **genuine** and **not lost/stolen** — worth adopting in Phase 1.
