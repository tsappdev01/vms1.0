/*  011_add_card_image_blob.sql
    Lets vms.VisitorCardImage hold a blob name instead of the bytes, for deployments with
    an Azure storage account.

    Run before deploying the build that writes it. DbBootstrapper checks the model's
    columns against the database at startup and refuses to run if any are missing, so
    without this the app will not start - the intended failure, and better than
    "Invalid column name 'BlobName'" on the first save at the desk.

    Re-runnable.

    -------------------------------------------------------------------------------
    Why the image moves out of the database

    A card image is 30-40 KB of opaque bytes that nothing queries, joins or indexes. In SQL
    those bytes are in every backup, every restore, and every DTU the database is billed
    for; a year of a busy desk is a database that is mostly pictures. In blob they cost a
    fraction of that, they never slow a query down, and a lifecycle rule can move them to
    cool storage or delete them at an age without a migration.

    Everything else about the visit stays where it is. The blob holds only what the card
    looked like; who visited whom is still one row in vms.VisitorEntry.

    The name is stored, not a URL. The account, the container and the credential are
    deployment settings; a URL in the data would pin all three into it and break every
    historic row the day any of them changes.

    -------------------------------------------------------------------------------
    Why Image becomes nullable rather than being dropped

    Rows written before this script have their bytes in the column, and they must go on
    working - including on UATWEB01, which has no storage account and where the column
    stays the only place an image goes. So a row carries exactly one of the two, and the
    application reads whichever it finds.

    No CHECK constraint enforcing that. The rule is enforced where the row is made -
    CardImageStore returns null rather than an empty row - and a constraint here would be a
    second definition of it, written by hand, in a schema whose whole point is that there
    is one definition generated from the model.
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

IF OBJECT_ID('vms.VisitorCardImage', 'U') IS NULL
BEGIN
    RAISERROR('vms.VisitorCardImage does not exist. Run 009_add_card_image.sql first.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('vms.VisitorCardImage', 'U')
                 AND name = 'BlobName')
BEGIN
    ALTER TABLE vms.VisitorCardImage ADD BlobName nvarchar(400) NULL;
    PRINT 'Added vms.VisitorCardImage.BlobName.';
END
ELSE
    PRINT 'vms.VisitorCardImage.BlobName already exists.';
GO

/*  Its own batch, after the ADD above: a batch is compiled as a whole, so touching a column
    added in the same batch fails the batch and takes the ADD with it. */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('vms.VisitorCardImage', 'U')
             AND name = 'Image'
             AND is_nullable = 0)
BEGIN
    ALTER TABLE vms.VisitorCardImage ALTER COLUMN Image varbinary(max) NULL;
    PRINT 'vms.VisitorCardImage.Image is now nullable - a row in blob storage has no bytes here.';
END
ELSE
    PRINT 'vms.VisitorCardImage.Image is already nullable.';
GO

SELECT
    (SELECT COUNT(*) FROM vms.VisitorCardImage)                                AS CardImages,
    (SELECT COUNT(*) FROM vms.VisitorCardImage WHERE BlobName IS NOT NULL)     AS InBlobStorage,
    (SELECT COUNT(*) FROM vms.VisitorCardImage WHERE Image IS NOT NULL)        AS InTheDatabase,
    (SELECT CAST(ISNULL(SUM(DATALENGTH(Image)), 0) / 1048576.0 AS decimal(10, 2))
     FROM vms.VisitorCardImage)                                                AS DatabaseMB;
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
