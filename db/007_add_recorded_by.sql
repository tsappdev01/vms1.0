/*  007_add_recorded_by.sql
    Adds vms.VisitorEntry.RecordedBy - who saved the entry, as Entra ID knows them.

    Run on UATWEB01 before deploying the build that signs users in. DbBootstrapper checks
    the model's columns against the database at startup and refuses to run if any are
    missing, so without this the app will not start - which is the intended failure, and
    better than a save that fails at the desk.

    Re-runnable. Nullable on purpose: every row written before sign-in existed has no
    answer, and null says that. Backfilling a name would be inventing an audit trail.
*/

USE VMS;
GO

IF COL_LENGTH('vms.VisitorEntry', 'RecordedBy') IS NULL
BEGIN
    ALTER TABLE vms.VisitorEntry ADD RecordedBy nvarchar(256) NULL;
    PRINT 'Added vms.VisitorEntry.RecordedBy.';
END
ELSE
    PRINT 'vms.VisitorEntry.RecordedBy already exists.';
GO

/* The report groups by entity and orders by time; this is for the other question an
   audit asks - what did one person record. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VisitorEntry_RecordedBy'
               AND object_id = OBJECT_ID('vms.VisitorEntry'))
BEGIN
    CREATE INDEX IX_VisitorEntry_RecordedBy ON vms.VisitorEntry (RecordedBy) INCLUDE (RecordedAtUtc);
    PRINT 'Created IX_VisitorEntry_RecordedBy.';
END
ELSE
    PRINT 'IX_VisitorEntry_RecordedBy already exists.';
GO
