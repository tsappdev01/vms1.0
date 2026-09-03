# VMS Documentation

Design documentation for the Dubai Investments Visitor Management System.

## Current

| Document | Contents |
|---|---|
| [vms-design.html](vms-design.html) | **Design of record.** The system as built: why it runs at the desk, the capture path, the two screens, the data model, the constraints discovered rather than chosen, and the security gaps stated plainly. Open it in a browser. |
| [deployment.md](deployment.md) | **How to deploy it, and where it can run.** The reader is local, so the app is: the reception-PC runbook, and what a central server would cost. |
| [00-sdk-analysis.md](00-sdk-analysis.md) | Analysis of the ICP ID Card Toolkit v3.1.6. Still current — it is SDK fact, not architecture. |
| [icp-support-request.md](icp-support-request.md) | The licence escalation sent to ICP, and what came back. |

## Superseded

Documents 01–07 describe the Android, React and REST architecture that preceded the
reset. That design no longer exists; they are kept for the requirement tracing and the
BRD section references, not as a description of the system.

| Document | Contents |
|---|---|
| [01-architecture.md](01-architecture.md) | Components, projects, stack, database target. |
| [02-data-model.md](02-data-model.md) | Entities, lifecycle, indexing, deviations from BRD §17. |
| [03-api-specification.md](03-api-specification.md) | REST contract and authorisation matrix. |
| [04-reception-app-design.md](04-reception-app-design.md) | Android reception client and the ID scan flow. |
| [05-web-portal-design.md](05-web-portal-design.md) | Security/Admin web portal. |
| [06-security-privacy-rbac.md](06-security-privacy-rbac.md) | Roles, ID masking, encryption, audit, retention. |
| [07-implementation-plan.md](07-implementation-plan.md) | Phasing, prerequisites and current status. |

## Headline findings

1. Emirates ID is read **from the card's chip**, not by OCR. On Windows **any PC/SC reader** works; on mobile the reader must match a shipped plugin.
2. Toolkits exist for Android, iOS and Windows. MAUI is viable but needs a binding layer per mobile platform; the Windows .NET binding needs none.
3. Every device must be **registered with the ICP Validation Gateway** against a Service Provider licence, which **expires**.
4. The supplied configs are **QA**; production configs are needed from ICP.
5. The SDK can prove a card is **genuine** and **not lost/stolen** — worth adopting in Phase 1.
