SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* =============================================================================
   Development / UAT master data.

   Entities are from BRD section 6; the hosts and locations reproduce the worked
   examples in sections 4 and 5 (John Smith, Finance, floor 8, office 801) so the
   documented flows can be exercised as written.

   Re-runnable: every insert is guarded on a fixed GUID, so running this twice
   changes nothing and existing edits are never overwritten.

   NOT for production. Real hosts come from an HR extract or an Entra ID sync;
   real users come from Entra ID.
   ============================================================================= */

DECLARE @System UNIQUEIDENTIFIER = '00000000-0000-0000-0000-0000000000FF';

/* ------------------------------------------------------------------- Users
   The four role identities the development authentication stand-in presents.
   Their GUIDs match DevCurrentUser exactly (role number in the last octet), so
   audit rows written in development resolve to a named user. */

MERGE vms.[User] AS t
USING (VALUES
    ('00000000-0000-0000-0000-000000000001', N'security01',   N'Yusuf Kamal',    1, N'DIP Main Reception', 0),
    ('00000000-0000-0000-0000-000000000002', N'supervisor01', N'Layla Haddad',   2, N'DIP Main Reception', 1),
    ('00000000-0000-0000-0000-000000000003', N'admin01',      N'Imran Qureshi',  3, NULL,                  0),
    ('00000000-0000-0000-0000-000000000004', N'sysadmin',     N'Deepa Menon',    4, NULL,                  1),
    ('00000000-0000-0000-0000-0000000000FF', N'system',       N'System',         4, NULL,                  0)
) AS s (Id, Username, Name, Role, SecurityLocation, CanViewUnmaskedId)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, Username, Name, Role, SecurityLocation, CanViewUnmaskedId, IsActive, CreatedByUserId)
    VALUES (s.Id, s.Username, s.Name, s.Role, s.SecurityLocation, s.CanViewUnmaskedId, 1, '00000000-0000-0000-0000-0000000000FF');
GO

DECLARE @System UNIQUEIDENTIFIER = '00000000-0000-0000-0000-0000000000FF';

/* -------------------------------------------------------------- DI entities */

MERGE vms.DiEntity AS t
USING (VALUES
    ('10000000-0000-0000-0000-000000000001', N'DIPJSC', N'Dubai Investments PJSC'),
    ('10000000-0000-0000-0000-000000000002', N'DIP',    N'Dubai Investments Park'),
    ('10000000-0000-0000-0000-000000000003', N'NGI',    N'National General Insurance'),
    ('10000000-0000-0000-0000-000000000004', N'MSH',    N'Masharie'),
    ('10000000-0000-0000-0000-000000000005', N'GLASS',  N'Glass Entities'),
    ('10000000-0000-0000-0000-000000000006', N'SUBS',   N'Other DI Subsidiaries'),
    ('10000000-0000-0000-0000-000000000007', N'SISTER', N'Other DI Sister Companies')
) AS s (Id, EntityCode, EntityName)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, EntityCode, EntityName, IsActive, CreatedByUserId)
    VALUES (s.Id, s.EntityCode, s.EntityName, 1, @System);
GO

DECLARE @System UNIQUEIDENTIFIER = '00000000-0000-0000-0000-0000000000FF';

/* ------------------------------------------------------- Building and floors */

MERGE vms.Building AS t
USING (VALUES
    ('20000000-0000-0000-0000-000000000001', N'DI Head Office', N'Dubai Investments Park, Dubai')
) AS s (Id, BuildingName, Location)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, BuildingName, Location, IsActive, CreatedByUserId)
    VALUES (s.Id, s.BuildingName, s.Location, 1, @System);

MERGE vms.Floor AS t
USING (VALUES
    ('30000000-0000-0000-0000-000000000003', N'3',  N'Third Floor'),
    ('30000000-0000-0000-0000-000000000005', N'5',  N'Fifth Floor'),
    ('30000000-0000-0000-0000-000000000008', N'8',  N'Eighth Floor'),
    ('30000000-0000-0000-0000-000000000012', N'12', N'Twelfth Floor')
) AS s (Id, FloorNo, FloorName)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, BuildingId, FloorNo, FloorName, CreatedByUserId)
    VALUES (s.Id, '20000000-0000-0000-0000-000000000001', s.FloorNo, s.FloorName, @System);
GO

DECLARE @System UNIQUEIDENTIFIER = '00000000-0000-0000-0000-0000000000FF';

/* ------------------------------------------------------------------ Offices */

MERGE vms.Office AS t
USING (VALUES
    ('40000000-0000-0000-0000-000000000801', '30000000-0000-0000-0000-000000000008', N'801',  N'Finance'),
    ('40000000-0000-0000-0000-000000000512', '30000000-0000-0000-0000-000000000005', N'512',  N'Legal'),
    ('40000000-0000-0000-0000-000000001203', '30000000-0000-0000-0000-000000000012', N'1203', N'Operations'),
    ('40000000-0000-0000-0000-000000000305', '30000000-0000-0000-0000-000000000003', N'305',  N'Claims')
) AS s (Id, FloorId, OfficeNo, Department)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, FloorId, OfficeNo, Department, CreatedByUserId)
    VALUES (s.Id, s.FloorId, s.OfficeNo, s.Department, @System);
GO

DECLARE @System UNIQUEIDENTIFIER = '00000000-0000-0000-0000-0000000000FF';

/* ------------------------------------------------------------ Hosts (BRD 5) */

MERGE vms.Employee AS t
USING (VALUES
    ('50000000-0000-0000-0000-000000000001', N'DI-1042', N'John Smith',
     '10000000-0000-0000-0000-000000000001', N'Finance',    N'Finance Manager',
     '30000000-0000-0000-0000-000000000008', '40000000-0000-0000-0000-000000000801',
     N'john.smith@example.ae',      N'+971 50 000 0001'),
    ('50000000-0000-0000-0000-000000000002', N'DI-1088', N'Sarah Ahmed',
     '10000000-0000-0000-0000-000000000001', N'Legal',      N'Legal Counsel',
     '30000000-0000-0000-0000-000000000005', '40000000-0000-0000-0000-000000000512',
     N'sarah.ahmed@example.ae',     N'+971 50 000 0002'),
    ('50000000-0000-0000-0000-000000000003', N'DI-1190', N'Peter Thomas',
     '10000000-0000-0000-0000-000000000004', N'Operations', N'Operations Head',
     '30000000-0000-0000-0000-000000000012', '40000000-0000-0000-0000-000000001203',
     N'peter.thomas@example.ae',    N'+971 50 000 0003'),
    ('50000000-0000-0000-0000-000000000004', N'NGI-233', N'Fatima Al Marri',
     '10000000-0000-0000-0000-000000000003', N'Claims',     N'Claims Supervisor',
     '30000000-0000-0000-0000-000000000003', '40000000-0000-0000-0000-000000000305',
     N'fatima.almarri@example.ae',  N'+971 50 000 0004')
) AS s (Id, EmployeeCode, Name, DiEntityId, Department, Designation, FloorId, OfficeId, Email, Mobile)
ON t.Id = s.Id
WHEN NOT MATCHED THEN
    INSERT (Id, EmployeeCode, Name, DiEntityId, Department, Designation, FloorId, OfficeId, Email, Mobile, IsActive, CreatedByUserId)
    VALUES (s.Id, s.EmployeeCode, s.Name, s.DiEntityId, s.Department, s.Designation, s.FloorId, s.OfficeId, s.Email, s.Mobile, 1, @System);
GO

PRINT 'Seed complete.';
SELECT 'DiEntity' AS TableName, COUNT(*) AS Rows FROM vms.DiEntity
UNION ALL SELECT 'Building', COUNT(*) FROM vms.Building
UNION ALL SELECT 'Floor',    COUNT(*) FROM vms.Floor
UNION ALL SELECT 'Office',   COUNT(*) FROM vms.Office
UNION ALL SELECT 'Employee', COUNT(*) FROM vms.Employee
UNION ALL SELECT 'User',     COUNT(*) FROM vms.[User];
GO
