# Backup & Recovery Procedure

**Visitor Management System · Dubai Investments PJSC**
DI-IT-POL-AIDEV-001 §6 — Backup & Recovery Procedure (Controlled, required · IT)

**This procedure is not complete until §1 is decided and §5 has been executed once.** An
untested backup is a belief, not a control.

---

## 1 · What has to be decided

Two numbers and one period. They are business decisions, not technical ones.

| | Question | Decided |
|---|---|---|
| **RPO** — recovery point objective | How much recorded visitor data may be lost? A full working day, or an hour? | `[ ]` |
| **RTO** — recovery time objective | How long may reception be unable to check anybody in? | `[ ]` |
| **Backup retention** | How long are backups kept? Note this cannot be longer than the data retention period once one is set — see `08-data-classification.md` §5 | `[ ]` |

> **[DECISION REQUIRED — Business Owner and IT Head]**
>
> A sensible starting point to argue from: **RPO 1 hour, RTO 4 hours.** The reason RTO can
> be measured in hours rather than minutes is that reception is not blocked by an outage —
> they can record visits on paper and enter them afterwards. The reason RPO should be short
> is that a lost visit is a person who was in the building and is not in the record.

## 2 · What is backed up

| | What | How | Owner |
|---|---|---|---|
| **Azure SQL** (`ts-db` / `vms`) | Every visit, person and entity | Azure SQL automated backups — point-in-time restore, geo-redundant by default | Microsoft, configured by IT |
| **SQL Server on UATWEB01** (`VMS`) | The same | **IT's SQL Server backup job.** Confirm it covers this database | IT |
| **Azure Blob Storage** | Card images, where a storage account is configured | `[ ]` Soft delete and versioning — confirm both are on | IT |
| **Card images in the database** | Where there is no storage account | Covered by the database backup | — |
| **Source code** | Everything | GitHub `tsappdev01/vms1.0`, plus every developer's clone | GitHub |
| **Configuration** | Connection strings, keys, toolkit settings | **Azure app settings are *not* in the database backup.** Export them | IT |
| **ICP toolkit and licence** | The SDK and the Service Provider licence | In the repository (SDK) and with ICP (licence) | IT / ICP |

> **The gap people miss is configuration.** Restoring the database gets the data back and
> leaves an application that cannot start, because the connection string, the Entra client
> secret, the storage key and the three tablet API keys are in app settings. Export them
> whenever they change and keep the export where the IT credential store is.

## 3 · What does *not* need backing up

- **The tablets.** They hold a server address, an API key and a PIN hash — nothing else. A
  lost tablet is re-set-up from `16-admin-guide.md` §6 in a few minutes.
- **The reception PCs.** The application is deployed from the repository; the ICP agent is
  installed from `deploy/`.
- **Table schema.** It is created from the EF model at startup.

## 4 · Recovery

### 4.1 Database lost or corrupted

1. Stop the application so nothing writes while you restore.
2. Restore to the chosen point in time (Azure SQL point-in-time restore, or IT's SQL Server
   restore on UATWEB01).
3. Confirm the application's SQL login still exists — re-run `db/006_grant_app_login.sql`
   or `db/008_grant_app_user_azure.sql` as appropriate.
4. Start the application and read the startup log. The bootstrapper will report anything
   absent; a missing migration stops startup **naming the script**.
5. `GET /health` → healthy.
6. Check the newest visit against reception's own record of that day.
7. **Any visit recorded after the restore point is gone.** Ask reception to re-enter from
   their paper record.

### 4.2 Application host lost

1. Re-deploy from the repository with the script for that host (`05-infrastructure.md` §3).
2. Restore the exported configuration into app settings or `appsettings.Production.json`.
3. `GET /health` → healthy.

The database is untouched by this. Nothing is lost.

### 4.3 Card images lost

Visits remain complete and readable. The card image is evidence of the read, not part of
the record — the fields were stored separately. Restore blob storage from soft delete where
it is on; otherwise the images for that period are gone and the visits are not.

### 4.4 Tablet lost or stolen

1. **Rotate that tablet's API key immediately**, server-side (`16-admin-guide.md` §5). The
   key is behind a PIN on the device, but a rotated key is the control that does not depend
   on the PIN holding.
2. Leave the other two tablets' keys alone — this is what one key per tablet is for.
3. Record it on `19-incident-response.md`.
4. **No visitor data is on the device.** Nothing is disclosed by the loss itself.

### 4.5 ICP licence expired

Card reading stops everywhere at once. **This is not a recovery scenario, it is a diary
failure** — see R-06. Reception falls back to manual entry, which needs nothing from ICP,
while IT renews with ICP.

## 5 · Restore test

**A backup nobody has restored is not a backup.**

| | |
|---|---|
| Frequency | Quarterly, and once before go-live (`14-go-live-approval.md` T14) |
| Method | Restore to a scratch database, point the application at it, check in a test visitor, read the report |
| Evidence | The restore log and a screenshot of the report, attached to the quarterly review work item |

| Date | Restored by | Point restored to | Time taken | Result |
|---|---|---|---|---|
| | | | | |

## 6 · Ownership

| | Name | |
|---|---|---|
| Backup owner | `[ ]` | |
| Restore authorised by | `[ ]` | |
| Escalation | `[ ]` | `19-incident-response.md` |
