/* ==========================================================================
   VMS - the group companies the address list has but the entity list does not
   Server UATWEB01, database VMS.

   OPTIONAL, and a decision rather than a fix.

   The AD export covers 725 people across 26 companies. Twelve of those
   companies map onto the entity list; 13 do not, and they account for
   436 people - the majority of the export:

       189  Emirates Building System
        98  Emirates Glass
        48  Emirates Extrusion Factory
        29  White Aluminium Extrusion
        17  Gulf Metal Craft
        16  Lite-Tech Industries
        13  Al Taif Investment
        12  Emirates Extruded Polystyrene
         8  DIP Angola
         2  Mirdif Hills
         2  DI Investment Holding Limited
         1  Palisades
         1  Al Mal Capital

   Until an entity exists for them:

     - Those people have a null DiEntityId, so the Person to visit picker finds
       them only when "Search all entities" is ticked. They are never lost, but
       they are not the default.
     - "Entity being visited" cannot name their company at all, so a visit to
       Emirates Building System has to be filed against something else.

   That second point is the one that matters. If those companies receive visitors
   at the DIP desk, run this. If the eleven entities are deliberately the only
   ones that do, do not - and the picker will keep finding everyone else under
   "Search all entities".

   Names are exactly as the export spells them, so 004 links them with no edit to
   its mapping: re-run 004 afterwards to attach the people.

   Re-runnable. Run it with either of:
       sqlcmd -S UATWEB01 -E -i db\005_add_group_companies.sql
       - or open it in SSMS against VMS and execute.
   ========================================================================== */

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

INSERT INTO vms.Entity (Name, IsActive)
SELECT v.Name, 1
FROM (VALUES
    (N'Emirates Building System'),   -- 189 people
    (N'Emirates Glass'),   -- 98 people
    (N'Emirates Extrusion Factory'),   -- 48 people
    (N'White Aluminium Extrusion'),   -- 29 people
    (N'Gulf Metal Craft'),   -- 17 people
    (N'Lite-Tech Industries'),   -- 16 people
    (N'Al Taif Investment'),   -- 13 people
    (N'Emirates Extruded Polystyrene'),   -- 12 people
    (N'DIP Angola'),   -- 8 people
    (N'Mirdif Hills'),   -- 2 people
    (N'DI Investment Holding Limited'),   -- 2 people
    (N'Palisades'),   -- 1 people
    (N'Al Mal Capital')   -- 1 people
) AS v (Name)
WHERE NOT EXISTS (SELECT 1 FROM vms.Entity e WHERE e.Name = v.Name);
GO

SELECT Name, IsActive FROM vms.Entity ORDER BY IsActive DESC, Name;
GO

/* Clears the guard at the top, so a session that ran this against the wrong database is
   not left refusing to execute anything afterwards. */
SET NOEXEC OFF;
GO
