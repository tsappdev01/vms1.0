/*  007_add_recorded_by.sql
    Adds vms.VisitorEntry.RecordedBy - who saved the entry, as Entra ID knows them.

    Run on UATWEB01 before deploying the build that signs users in. DbBootstrapper checks
    the model's columns against the database at startup and refuses to run if any are
    missing, so without this the app will not start - which is the intended failure, and
    better than a save that fails at the desk.

    Re-runnable. Nullable on purpose: every row written before sign-in existed has no
    answer, and null says that. Backfilling a name would be inventing an audit trail.
*/

/*  No USE statement, deliberately: Azure SQL does not support switching databases, and
    the same file has to work whether it is run against SQL Server on UATWEB01 or against
    Azure SQL. So connect to the VMS database first - `sqlcmd -d VMS`, or choose it in the
    database dropdown in SSMS.

    The guard is here because the failure mode without it is silent: run against master by
    mistake and you get a set of vms.* objects in the wrong database, with nothing to say
    so until something else goes looking for them. SET NOEXEC ON leaves the rest of the
    file parsed but unexecuted, so nothing is half-applied. */
IF DB_NAME() IN (N'master', N'msdb', N'model', N'tempdb')
BEGIN
    RAISERROR('Connect to the VMS database before running this script. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
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

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
