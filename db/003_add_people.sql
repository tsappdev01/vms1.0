/* =============================================================================
   VMS - the people a visitor can come to see
   Server UATWEB01, database VMS.

   Creates vms.Person, and adds the host snapshot columns to vms.VisitorEntry.

   The rows themselves are NOT here. Commit the address list as db/people.csv and
   004_seed_people.sql will be generated from it - see db/README.md for the columns.

   Re-runnable: a second run adds nothing.

   Run it with either of:
       sqlcmd -S UATWEB01 -E -i db\003_add_people.sql
       - or open it in SSMS against VMS and execute.
   ============================================================================= */

USE VMS;
GO

/* -----------------------------------------------------------------------------
   1. vms.Person. Matches what Data/Entities.cs declares, so DbBootstrapper's
      column check passes and the two definitions do not drift.

      DiEntityId is nullable on purpose: the address list's company names and the
      entity list do not fully agree - "Emirates Building System" has no entity at
      all - and a person whose company cannot be matched must still be findable by
      searching all entities rather than vanishing.
   ----------------------------------------------------------------------------- */

IF OBJECT_ID(N'vms.Person', N'U') IS NULL
CREATE TABLE vms.Person
(
    Id          INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Person PRIMARY KEY,
    DisplayName NVARCHAR(200) NOT NULL,
    Title       NVARCHAR(200) NULL,
    Email       NVARCHAR(256) NULL,
    CompanyName NVARCHAR(200) NULL,
    DiEntityId  INT           NULL CONSTRAINT FK_Person_Entity REFERENCES vms.Entity (Id),
    IsActive    BIT           NOT NULL CONSTRAINT DF_Person_IsActive DEFAULT 1
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Person_DisplayName' AND object_id = OBJECT_ID(N'vms.Person'))
CREATE INDEX IX_Person_DisplayName ON vms.Person (DisplayName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Person_DiEntityId_IsActive' AND object_id = OBJECT_ID(N'vms.Person'))
CREATE INDEX IX_Person_DiEntityId_IsActive ON vms.Person (DiEntityId, IsActive);
GO

/* -----------------------------------------------------------------------------
   2. The host snapshot on the visit.

      PersonToVisitId says a host was picked from the list rather than typed. The
      other three are copied at the time of the visit rather than joined later: a
      host who changes title, or leaves, must not silently rewrite what an earlier
      report said. NO ACTION on the foreign key, so removing someone from the
      address list can never delete the visits that came to see them.
   ----------------------------------------------------------------------------- */

IF COL_LENGTH('vms.VisitorEntry', 'PersonToVisitId') IS NULL
    ALTER TABLE vms.VisitorEntry ADD PersonToVisitId INT NULL;
GO

IF COL_LENGTH('vms.VisitorEntry', 'PersonToVisitTitle') IS NULL
    ALTER TABLE vms.VisitorEntry ADD PersonToVisitTitle NVARCHAR(200) NULL;
GO

IF COL_LENGTH('vms.VisitorEntry', 'PersonToVisitEmail') IS NULL
    ALTER TABLE vms.VisitorEntry ADD PersonToVisitEmail NVARCHAR(256) NULL;
GO

IF COL_LENGTH('vms.VisitorEntry', 'PersonToVisitCompany') IS NULL
    ALTER TABLE vms.VisitorEntry ADD PersonToVisitCompany NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VisitorEntry_Person')
    ALTER TABLE vms.VisitorEntry
        ADD CONSTRAINT FK_VisitorEntry_Person
        FOREIGN KEY (PersonToVisitId) REFERENCES vms.Person (Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_VisitorEntry_PersonToVisitId' AND object_id = OBJECT_ID(N'vms.VisitorEntry'))
CREATE INDEX IX_VisitorEntry_PersonToVisitId ON vms.VisitorEntry (PersonToVisitId);
GO

PRINT N'vms.Person and the host snapshot columns are in place. It is empty until 004 is run.';
SELECT COUNT(*) AS People FROM vms.Person;
GO
