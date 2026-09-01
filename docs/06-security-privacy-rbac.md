# Security, Privacy and RBAC

Implements BRD §18, §19 and §22. Emirates ID data is sensitive personal data sourced from
a government chip, so ICP's terms may be stricter than DI's own policy — **where they
differ, the tighter one applies.**

## Roles (BRD §18)

| Role | Capabilities |
|---|---|
| Security Officer | Register, check-in, check-out, search, current visitors, emergency list |
| Security Supervisor | + correct transactions, reports, all reception points |
| Admin | + masters (entities, hosts, floors/offices), users, configuration |
| System Administrator | + system config, integrations, security, audit administration |

Full endpoint matrix in [`03-api-specification.md`](03-api-specification.md).

`CanViewUnmaskedId` is a **separate per-user grant**, not implied by any role. §22 requires
normal Security users to see only `784-XXXX-XXXXXXX-X`, and seniority is not the same thing
as a need to see the number.

## ID number handling

Three representations, and only one of them is the real value:

| Form | Where | Who sees it |
|---|---|---|
| `IdNumberCipher` | Database | Nobody directly |
| `IdNumberHash` | Database | Nobody — lookup key only |
| Masked | API responses, UI, exports, logs, audit values | Everyone |
| Plaintext | Single dedicated endpoint | `CanViewUnmaskedId`, audited every time |

**Encryption.** Envelope encryption — AES-256-GCM data key, wrapped by a Key Vault master
key. The database never holds the master key, so a backup or a stolen `.bak` yields nothing.

**Hashing.** HMAC-SHA256 over the normalised number, under a server-side pepper held in
Key Vault. A plain unkeyed hash would be trivially brute-forced — the Emirates ID space is
small and structured (`784` + year + 7 digits + check digit), so an unkeyed hash is
effectively plaintext. The pepper is what makes the lookup index safe.

**Never logged.** Not in application logs, not in audit `OldValue`/`NewValue`, not in
exception messages. `IdNumber.ToString()` deliberately returns the *masked* form so that an
accidental string interpolation cannot leak it.

## Transport and device

- TLS 1.2+ everywhere; HSTS on the portal.
- Reception devices present a client certificate (mTLS) in addition to the user's Entra
  token — BRD §22 "device authentication". A stolen token alone is not enough.
- Certificates and VG registration IDs live in the Android Keystore.
- Idle auto-logout; kiosk mode; MDM-managed remote wipe.

## Audit (BRD §19)

Append-only. `INSERT` and `SELECT` granted; no `UPDATE` or `DELETE` outside the retention job.

Audited: check-in, check-out, visit amendment (with old/new — §19's host-change example),
master-data changes, user/role changes, login success and failure, **and every unmasked ID
view**. Each row carries user, timestamp, IP and device.

## Retention (BRD §22)

Retention period is a configuration value, to be set by DI Legal/Compliance against both
DI policy and ICP's terms. A nightly job:

1. Selects visits past retention.
2. Purges the signature image and the visitor photo.
3. Clears `IdNumberCipher`, keeping the hash only if repeat-recognition across the retention
   boundary is wanted — **this is a policy decision, not a technical one**, and it should be
   made explicitly rather than defaulted.
4. Stamps `PurgedAtUtc`; retains non-identifying visit facts for statistics.
5. Writes an audit row for the purge.

Secure deletion, not a soft flag — §22 asks for deletion after retention.

## Open compliance items

1. **Retention period** — the BRD leaves it undefined. Needed before go-live.
2. **ICP terms on chip-sourced data** — may cap retention below DI's own policy.
3. **Photograph** — §3 marks it optional/subject to policy. Confirm whether DI stores it at all.
4. **Cross-border** — if hosted in Azure, confirm UAE data residency is acceptable, or host
   on DI private infrastructure (§23 allows either).
5. **Visitor privacy notice** — the §7 signature is an acknowledgement; confirm with Legal
   that it constitutes valid consent for the data actually captured.
