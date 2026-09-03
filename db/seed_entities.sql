/* =============================================================================
   VMS - the entity list
   Server UATWEB01, database VMS.

   Populates vms.Entity, which is what the "Entity being visited" dropdown on the
   New Visitor screen reads. Nothing in the application seeds it: the list is data
   owned by the database, so it is changed here and picked up on a page refresh,
   with no rebuild and no redeploy.

   Re-runnable. A second run inserts nothing rather than failing.

   Run it with either of:
       sqlcmd -S UATWEB01 -E -i db\seed_entities.sql
       - or open it in SSMS against VMS and execute.

   Note: this script does not create vms.VisitorEntry, which the Save button needs.
   That table is created from the EF model by Data/DbBootstrapper.cs the first time
   the application starts, so that there is only one definition of its 30-odd
   columns to keep correct.
   ============================================================================= */

USE VMS;
GO

/* -----------------------------------------------------------------------------
   1. The table.
      Not needed if the application has already been started once - it creates
      this itself. Kept here so the entities can be loaded before that, and so
      the table can be rebuilt without the application. Deliberately matches what
      EF Core generates for the DiEntity type, constraint and index names
      included, so the two definitions agree.

      CREATE SCHEMA must be alone in its batch, hence EXEC.
   ----------------------------------------------------------------------------- */

IF SCHEMA_ID(N'vms') IS NULL EXEC(N'CREATE SCHEMA [vms];');
GO

IF OBJECT_ID(N'vms.Entity', N'U') IS NULL
CREATE TABLE vms.Entity
(
    Id       INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Entity PRIMARY KEY,
    Name     NVARCHAR(200) NOT NULL,
    IsActive BIT           NOT NULL CONSTRAINT DF_Entity_IsActive DEFAULT 1
);
GO

-- Its own batch: a CREATE INDEX in the same batch as the CREATE TABLE above cannot
-- be relied on to resolve the table name.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Entity_Name' AND object_id = OBJECT_ID(N'vms.Entity'))
CREATE UNIQUE INDEX IX_Entity_Name ON vms.Entity (Name);
GO

/* -----------------------------------------------------------------------------
   2. The entities, as they appear in the existing employee-company dropdown.

      NOT EXISTS compares under the database collation, which is case-insensitive
      - the same rule IX_Entity_Name enforces - so this cannot insert a row the
      index would then reject.

      That screenshot was cut off after TechSource, and the list looks
      alphabetical, so anything sorting after T may be missing. Add it to the
      VALUES list and re-run.
   ----------------------------------------------------------------------------- */

INSERT INTO vms.Entity (Name, IsActive)
SELECT v.Name, 1
FROM (VALUES
    (N'ALMujama'),
    (N'DanahBay'),
    (N'DI'),
    (N'DII'),
    (N'DIP'),
    (N'DIR'),
    (N'GlassLLC'),
    (N'Masharie'),
    (N'PI'),
    (N'PIDOA'),
    (N'TechSource')
) AS v (Name)
WHERE NOT EXISTS (SELECT 1 FROM vms.Entity e WHERE e.Name = v.Name);
GO

PRINT N'vms.Entity:';
SELECT Id, Name, IsActive FROM vms.Entity ORDER BY Name;
GO

/* -----------------------------------------------------------------------------
   Maintaining the list afterwards
   -----------------------------------------------------------------------------

   -- Add one. IsActive defaults to 1.
   INSERT INTO vms.Entity (Name) VALUES (N'NewCompany');

   -- Take one out of the dropdown, keeping its visitor history intact.
   UPDATE vms.Entity SET IsActive = 0 WHERE Name = N'DIR';

   -- Put it back.
   UPDATE vms.Entity SET IsActive = 1 WHERE Name = N'DIR';

   -- Correct a spelling. The dropdown and every report follow it, because visits
   -- reference the row rather than the text.
   UPDATE vms.Entity SET Name = N'AlMujama' WHERE Name = N'ALMujama';

   Prefer IsActive = 0 over DELETE. vms.VisitorEntry has a foreign key to these
   rows, so deleting an entity that has visits behind it is rejected - and if it
   has none, deactivating it costs nothing. The report's entity filter keeps a
   deactivated entity for as long as it still has visits, so old reports can
   still be filtered by it.
   ----------------------------------------------------------------------------- */
