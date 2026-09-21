/*  010_add_person_directory_object_id.sql
    Adds vms.Person.DirectoryObjectId - the Entra ID object ID, for the rows that come
    from the SSO directory rather than from the AD export.

    Run before deploying the build that reads the host list from Entra ID. DbBootstrapper
    checks the model's columns against the database at startup and refuses to run if any
    are missing, so without this the app will not start - the intended failure, and better
    than "Invalid column name 'DirectoryObjectId'" on the first search at the desk.

    Re-runnable.

    -------------------------------------------------------------------------------
    What changes, and what does not

    The host list at the desk now comes from Entra ID - the same directory people sign in
    with - so a joiner appears without a new export and a leaver stops appearing without
    one either. vms.Person stops being that list.

    It does not stop being needed. vms.VisitorEntry.PersonToVisitId points at it, and a
    visit has to stay readable years after the person has left the tenant and Graph no
    longer returns them at all. So it becomes a record of the people who have actually been
    visited: a row is written the first time somebody is picked out of the directory, and
    kept afterwards. The rows the export already loaded stay exactly as they are and are
    adopted - matched once on email address - rather than duplicated.

    -------------------------------------------------------------------------------
    Why the index is unique and filtered

    Unique because two rows for one person would split their visit history in two, and the
    application's upsert matches on this column - a duplicate would make which row it finds
    a matter of luck. Filtered to NOT NULL because every row loaded from the export has no
    object ID, and under a plain unique index the second such row would fail: SQL Server
    treats NULLs as equal for uniqueness.

    Left alone deliberately: IsActive and DiEntityId. Both are maintained against this
    table - IsActive by whoever retires a row, DiEntityId by the company-to-entity mapping
    in db/tools/generate_seed_people.py - and the directory has no equivalent of either
    that could be trusted to overwrite them.
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

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('vms.Person', 'U')
                 AND name = 'DirectoryObjectId')
BEGIN
    ALTER TABLE vms.Person ADD DirectoryObjectId nvarchar(64) NULL;
    PRINT 'Added vms.Person.DirectoryObjectId.';
END
ELSE
    PRINT 'vms.Person.DirectoryObjectId already exists.';
GO

/*  Its own batch. ALTER TABLE ... ADD is not visible to the rest of the batch it is in, so
    an index over the new column in the same batch fails to compile the whole batch - and
    takes the ALTER with it. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID('vms.Person', 'U')
                 AND name = 'IX_Person_DirectoryObjectId')
BEGIN
    CREATE UNIQUE INDEX IX_Person_DirectoryObjectId
        ON vms.Person (DirectoryObjectId)
        WHERE DirectoryObjectId IS NOT NULL;

    PRINT 'Created IX_Person_DirectoryObjectId.';
END
ELSE
    PRINT 'IX_Person_DirectoryObjectId already exists.';
GO

SELECT
    (SELECT COUNT(*) FROM vms.Person)                                          AS People,
    (SELECT COUNT(*) FROM vms.Person WHERE DirectoryObjectId IS NOT NULL)      AS FromDirectory,
    (SELECT COUNT(*) FROM vms.Person WHERE DirectoryObjectId IS NULL)          AS FromExport;
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
