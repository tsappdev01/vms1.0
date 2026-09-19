/* =============================================================================
   VMS - Purpose of Visit
   Server UATWEB01, database VMS.

   Adds Purpose and PurposeOther to vms.VisitorEntry. Data/DbBootstrapper.cs creates
   tables that are absent but never alters ones that are present, so a column added to
   the model reaches an existing database only through this script. Startup checks for
   these columns and refuses to run without them, naming them, rather than letting the
   first query fail with "Invalid column name".

   Re-runnable: a second run adds nothing.

   Run it with either of:
       sqlcmd -S UATWEB01 -E -i db\002_add_visit_purpose.sql
       - or open it in SSMS against VMS and execute.
   ============================================================================= */

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

/* -----------------------------------------------------------------------------
   Purpose. NOT NULL, because every visit has one and the screen requires it -
   so existing rows, recorded before the field existed, are backfilled as
   'Not recorded' rather than left to a default that would read as a real answer.
   ----------------------------------------------------------------------------- */

IF COL_LENGTH('vms.VisitorEntry', 'Purpose') IS NULL
BEGIN
    ALTER TABLE vms.VisitorEntry ADD Purpose NVARCHAR(60) NULL;
END
GO

UPDATE vms.VisitorEntry SET Purpose = N'Not recorded' WHERE Purpose IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('vms.VisitorEntry')
             AND name = 'Purpose' AND is_nullable = 1)
BEGIN
    ALTER TABLE vms.VisitorEntry ALTER COLUMN Purpose NVARCHAR(60) NOT NULL;
END
GO

/* -----------------------------------------------------------------------------
   PurposeOther. Nullable: it holds what was typed when the purpose was 'Other',
   and is nothing at all for every other purpose.
   ----------------------------------------------------------------------------- */

IF COL_LENGTH('vms.VisitorEntry', 'PurposeOther') IS NULL
BEGIN
    ALTER TABLE vms.VisitorEntry ADD PurposeOther NVARCHAR(200) NULL;
END
GO

SELECT c.name AS Column_Name, t.name AS Type, c.max_length / 2 AS Chars, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('vms.VisitorEntry') AND c.name IN ('Purpose', 'PurposeOther');
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
