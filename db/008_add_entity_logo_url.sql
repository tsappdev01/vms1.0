/*  008_add_entity_logo_url.sql
    Adds vms.Entity.LogoUrl - where each unit's logo is hosted, for anything outside this
    application to use: an external site, a self-service portal.

    Run on UATWEB01 before deploying the build that reads it. DbBootstrapper checks the
    model's columns against the database at startup and refuses to run if any are missing,
    so without this the app will not start - the intended failure, and better than finding
    out on the first page load.

    A URL rather than an uploaded file, because the consumer is not this application. A
    portal on another host wants an address it can put in an img tag, not a byte array
    behind an endpoint of ours that it would have to authenticate against.

    Re-runnable: the column, the constraint and each update are all guarded.
*/

USE VMS;
GO

IF COL_LENGTH('vms.Entity', 'LogoUrl') IS NULL
BEGIN
    ALTER TABLE vms.Entity ADD LogoUrl nvarchar(500) NULL;
    PRINT 'Added vms.Entity.LogoUrl.';
END
ELSE
    PRINT 'vms.Entity.LogoUrl already exists.';
GO

/*  https, or nothing.

    This value is embedded in pages this system does not control, which makes two things
    worth refusing at the column rather than trusting every consumer to check:

      - an http URL is blocked as mixed content on an https page, so the logo silently
        does not appear and nobody can tell whether the data or the portal is at fault;
      - a javascript: or data: URL rendered into an img or an anchor by a portal is a
        script injection into that portal, from our database.

    A CHECK constraint is the right place for it: the entity list is maintained by script
    and by hand, so there is no single application code path to put the check in.
*/
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Entity_LogoUrl_Https')
BEGIN
    ALTER TABLE vms.Entity WITH CHECK
        ADD CONSTRAINT CK_Entity_LogoUrl_Https
        CHECK (LogoUrl IS NULL OR LogoUrl LIKE 'https://%');
    PRINT 'Added CK_Entity_LogoUrl_Https.';
END
ELSE
    PRINT 'CK_Entity_LogoUrl_Https already exists.';
GO

/*  ---------------------------------------------------------------------------
    The values.

    One row per unit, matched on Name so this file stays readable and does not
    depend on identity values that differ between environments. Guarded so a
    re-run only writes what has changed, and so a name that is not in the list
    is reported rather than silently doing nothing.

    Add a line per unit as the URLs are supplied. The group logo below is the
    one that has been given so far.
    --------------------------------------------------------------------------- */

SET NOCOUNT ON;

DECLARE @logos TABLE (Name nvarchar(200) PRIMARY KEY, LogoUrl nvarchar(500) NOT NULL);

/* One INSERT per unit rather than a comma-separated list, so adding one is a copied line
   with no punctuation to get right. The names are the entity codes from 001 - DI, DIP,
   ALMujama and the rest - not the long company names. */

INSERT INTO @logos VALUES (N'DI',
    N'https://diweb.blob.core.windows.net/dubaiinvestmentcontainer/dip-images/public/1107/dubaiinvestments-logo-english.jpg');

-- INSERT INTO @logos VALUES (N'DIP',       N'https://…');
-- INSERT INTO @logos VALUES (N'DII',       N'https://…');
-- INSERT INTO @logos VALUES (N'DIR',       N'https://…');
-- INSERT INTO @logos VALUES (N'ALMujama',  N'https://…');
-- INSERT INTO @logos VALUES (N'DanahBay',  N'https://…');
-- INSERT INTO @logos VALUES (N'GlassLLC',  N'https://…');
-- INSERT INTO @logos VALUES (N'Masharie',  N'https://…');
-- INSERT INTO @logos VALUES (N'PI',        N'https://…');
-- INSERT INTO @logos VALUES (N'PIDOA',     N'https://…');
-- INSERT INTO @logos VALUES (N'TechSource', N'https://…');

UPDATE e
   SET e.LogoUrl = l.LogoUrl
  FROM vms.Entity e
  JOIN @logos l ON l.Name = e.Name
 WHERE e.LogoUrl IS NULL OR e.LogoUrl <> l.LogoUrl;

PRINT CONCAT('Logo URLs set: ', @@ROWCOUNT, '.');

/* A URL supplied for a unit that is not in the entity list is a mistake in one of the
   two, and silence would hide it. */
IF EXISTS (SELECT 1 FROM @logos l WHERE NOT EXISTS
           (SELECT 1 FROM vms.Entity e WHERE e.Name = l.Name))
BEGIN
    PRINT 'These names are not in vms.Entity, so their logo was not set:';
    SELECT l.Name AS [Unmatched unit] FROM @logos l
     WHERE NOT EXISTS (SELECT 1 FROM vms.Entity e WHERE e.Name = l.Name);
END
GO

SELECT Id, Name, IsActive, LogoUrl FROM vms.Entity ORDER BY Name;
GO
