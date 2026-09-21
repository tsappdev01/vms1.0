/*  009_add_card_image.sql
    Adds vms.VisitorCardImage - the card as read, drawn as an image and kept with the
    visit.

    Run before deploying the build that writes it. DbBootstrapper checks the model's tables
    and columns against the database at startup and refuses to run if any are missing, so
    without this the app will not start - the intended failure, and better than a save that
    fails at the desk.

    Re-runnable.

    -------------------------------------------------------------------------------
    Why a table and not two columns on vms.VisitorEntry

    The visitor report loads whole VisitorEntry rows to render a table that shows neither
    the photograph nor the card. A 40 KB image on that entity would be 40 KB per row
    dragged across the wire every time somebody opens a month - and over the internet now
    that the app and the database are both in Azure. In its own table it is reached through
    a navigation, which EF loads only when something asks, and nothing but the download
    endpoint ever does.

    The visit's own Id is the primary key, so a visit has at most one image and there is no
    second identity to keep in step. ON DELETE CASCADE, because the image is part of the
    visit and means nothing without it.

    -------------------------------------------------------------------------------
    Size: an SVG card with the chip photograph embedded is roughly 30-40 KB. Only card
    reads have one - a manual entry never will. If it ever costs more than it is worth, the
    rows can be deleted without touching a visit, and the image is derivable from the
    columns that remain.
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
    CREATE TABLE vms.VisitorCardImage
    (
        VisitorEntryId int             NOT NULL,
        Image          varbinary(max)  NOT NULL,
        ContentType    nvarchar(100)   NOT NULL,

        CONSTRAINT PK_VisitorCardImage PRIMARY KEY CLUSTERED (VisitorEntryId),
        CONSTRAINT FK_VisitorCardImage_VisitorEntry FOREIGN KEY (VisitorEntryId)
            REFERENCES vms.VisitorEntry (Id) ON DELETE CASCADE
    );

    PRINT 'Created vms.VisitorCardImage.';
END
ELSE
    PRINT 'vms.VisitorCardImage already exists.';
GO

SELECT
    (SELECT COUNT(*) FROM vms.VisitorEntry)                                   AS Visits,
    (SELECT COUNT(*) FROM vms.VisitorCardImage)                               AS WithCardImage,
    (SELECT CAST(ISNULL(SUM(DATALENGTH(Image)), 0) / 1048576.0 AS decimal(10, 2))
     FROM vms.VisitorCardImage)                                               AS CardImageMB;
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
