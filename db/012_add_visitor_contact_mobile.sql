/*  012_add_visitor_contact_mobile.sql
    Adds vms.VisitorEntry.ContactMobile - the phone number the visitor gives at the desk.

    You probably do not need to run this. From the build that introduced the column,
    DbBootstrapper adds absent *nullable* columns itself at startup, and this is one. The
    script is here because SQL in this project lives in db/ whether or not the application
    also applies it, and because a DBA who would rather change a production database by
    hand than let an application do it should have the statement in front of them.

    It is exactly what the bootstrapper runs, so applying it first simply means the
    bootstrapper finds nothing to do.

    Re-runnable.

    -------------------------------------------------------------------------------
    Why this is not AddressMobile

    vms.VisitorEntry already has AddressMobile. That one is whatever the chip held in the
    card's home-address block - part of what the signed response said, and not the desk's
    to correct. Overwriting it with a number typed at the counter would mean a row marked
    "Source: Card" carried a field no card ever produced, and the report would have no way
    to tell the two apart afterwards.

    ContactMobile is the number for this visit. It is prefilled from the card when the card
    had one, which is rarely: on every card tested at DIP the home-address block came back
    empty, mobile included. So in practice this is a field reception types, and that is the
    point of it - a number that will actually reach the visitor today, rather than one the
    card has been carrying since it was issued.
*/

/*  No USE statement, deliberately: Azure SQL does not support switching databases, and the
    same file has to work whether it is run against SQL Server on UATWEB01 or against Azure
    SQL. So connect to the VMS database first - `sqlcmd -d VMS`, or choose it in the
    database dropdown in SSMS. */
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
               WHERE object_id = OBJECT_ID('vms.VisitorEntry', 'U')
                 AND name = 'ContactMobile')
BEGIN
    ALTER TABLE vms.VisitorEntry ADD ContactMobile nvarchar(40) NULL;
    PRINT 'Added vms.VisitorEntry.ContactMobile.';
END
ELSE
    PRINT 'vms.VisitorEntry.ContactMobile already exists.';
GO

SELECT
    (SELECT COUNT(*) FROM vms.VisitorEntry)                                   AS Visits,
    (SELECT COUNT(*) FROM vms.VisitorEntry WHERE ContactMobile IS NOT NULL)   AS WithAContactNumber,
    (SELECT COUNT(*) FROM vms.VisitorEntry WHERE AddressMobile IS NOT NULL)   AS WithAMobileOnTheCard;
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
