/*  013_add_visitor_sign_out.sql
    Adds the two columns a visitor sign-out needs, and the index the sign-out screen reads.

    The COLUMNS you probably do not need to run: they are nullable, and DbBootstrapper adds
    absent nullable columns itself at startup.

    The INDEX you do. The bootstrapper adds columns and nothing else - an index on a table
    with rows in it is a decision about when to take the write lock, and that belongs to
    whoever knows when the desk is quiet. Without it the sign-out screen still works; it just
    scans the visit table instead of seeking a few open rows.

    Re-runnable.

    -------------------------------------------------------------------------------
    Why "still inside" is a null and not a status

    SignedOutAtUtc null means the visitor has not been signed out. There is no Status column
    with 'In' and 'Out' in it, because two columns that must agree eventually disagree - and
    the report that matters here is the one somebody reads during a fire drill.

    Nothing closes an entry automatically. A visitor who leaves without telling reception
    stays open until a person signs them out. That is deliberate: a nightly job that closed
    yesterday's entries would produce an evacuation list that was tidy and wrong, and the
    building would trust it.
*/

IF DB_NAME() IN (N'master', N'msdb', N'model', N'tempdb')
BEGIN
    RAISERROR('Connect to the VMS database before running this script. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

IF OBJECT_ID('vms.VisitorEntry', 'U') IS NULL
BEGIN
    RAISERROR('vms.VisitorEntry does not exist. Start the application once - it creates the tables from the model.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('vms.VisitorEntry', 'U') AND name = 'SignedOutAtUtc')
BEGIN
    ALTER TABLE vms.VisitorEntry ADD SignedOutAtUtc datetimeoffset NULL;
    PRINT 'Added vms.VisitorEntry.SignedOutAtUtc.';
END
ELSE
    PRINT 'vms.VisitorEntry.SignedOutAtUtc already exists.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('vms.VisitorEntry', 'U') AND name = 'SignedOutBy')
BEGIN
    ALTER TABLE vms.VisitorEntry ADD SignedOutBy nvarchar(256) NULL;
    PRINT 'Added vms.VisitorEntry.SignedOutBy.';
END
ELSE
    PRINT 'vms.VisitorEntry.SignedOutBy already exists.';
GO

/*  Its own batch, after the ADDs: a batch is compiled as a whole, so indexing a column added
    in the same batch fails the batch and takes the ADD with it.

    Filtered on IS NULL because that is the only rowset anybody queries here - the visitors
    still in the building. A full index would carry every closed visit for the life of the
    system to answer a question about the few dozen open ones. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID('vms.VisitorEntry', 'U')
                 AND name = 'IX_VisitorEntry_StillInside')
BEGIN
    CREATE INDEX IX_VisitorEntry_StillInside
        ON vms.VisitorEntry (SignedOutAtUtc)
        WHERE SignedOutAtUtc IS NULL;
    PRINT 'Created IX_VisitorEntry_StillInside.';
END
ELSE
    PRINT 'IX_VisitorEntry_StillInside already exists.';
GO

SELECT
    (SELECT COUNT(*) FROM vms.VisitorEntry)                                AS Visits,
    (SELECT COUNT(*) FROM vms.VisitorEntry WHERE SignedOutAtUtc IS NULL)   AS StillInside,
    (SELECT COUNT(*) FROM vms.VisitorEntry WHERE SignedOutAtUtc IS NOT NULL) AS SignedOut;
GO

SET NOEXEC OFF;
GO
