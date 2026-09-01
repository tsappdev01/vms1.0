/* =============================================================================
   Verifies every object the baseline schema should have created.
   Safe to run any time. Reports MISSING for anything absent.
   ============================================================================= */

SET NOCOUNT ON;

DECLARE @expected TABLE (ObjectName SYSNAME, ObjectKind VARCHAR(20));

INSERT INTO @expected (ObjectName, ObjectKind) VALUES
    (N'vms.DiEntity',          'TABLE'),
    (N'vms.Building',          'TABLE'),
    (N'vms.Floor',             'TABLE'),
    (N'vms.Office',            'TABLE'),
    (N'vms.Employee',          'TABLE'),
    (N'vms.[User]',            'TABLE'),
    (N'vms.Visitor',           'TABLE'),
    (N'vms.Visit',             'TABLE'),
    (N'vms.VisitorSignature',  'TABLE'),
    (N'vms.AuditLog',          'TABLE'),
    (N'vms.NextVisitNumber',   'FUNCTION');

SELECT
    ObjectName,
    ObjectKind,
    CASE WHEN OBJECT_ID(ObjectName) IS NULL THEN '*** MISSING ***' ELSE 'ok' END AS Status
FROM @expected
ORDER BY ObjectKind, ObjectName;

DECLARE @indexes TABLE (TableName SYSNAME, IndexName SYSNAME);

INSERT INTO @indexes (TableName, IndexName) VALUES
    (N'vms.Employee', N'IX_Employee_Name_Active'),
    (N'vms.Visitor',  N'IX_Visitor_Name'),
    (N'vms.Visitor',  N'IX_Visitor_Company'),
    (N'vms.Visitor',  N'IX_Visitor_IdExpiry'),
    (N'vms.Visit',    N'IX_Visit_Inside'),
    (N'vms.Visit',    N'IX_Visit_Visitor_InTime'),
    (N'vms.Visit',    N'IX_Visit_Host_InTime'),
    (N'vms.Visit',    N'IX_Visit_Expected'),
    (N'vms.AuditLog', N'IX_AuditLog_Ts'),
    (N'vms.AuditLog', N'IX_AuditLog_Record'),
    (N'vms.AuditLog', N'IX_AuditLog_User');

SELECT
    i.TableName,
    i.IndexName,
    CASE WHEN EXISTS (SELECT 1 FROM sys.indexes x
                      WHERE x.name = i.IndexName
                        AND x.object_id = OBJECT_ID(i.TableName))
         THEN 'ok' ELSE '*** MISSING ***' END AS Status
FROM @indexes i
ORDER BY i.TableName, i.IndexName;

SELECT
    N'vms.VisitNumberSequence' AS ObjectName,
    'SEQUENCE' AS ObjectKind,
    CASE WHEN EXISTS (SELECT 1 FROM sys.sequences
                      WHERE name = N'VisitNumberSequence'
                        AND schema_id = SCHEMA_ID(N'vms'))
         THEN 'ok' ELSE '*** MISSING ***' END AS Status;

/* Columns added by later scripts. */
SELECT
    N'vms.Visitor.IdNumberMasked' AS ObjectName,
    'COLUMN' AS ObjectKind,
    CASE WHEN COL_LENGTH(N'vms.Visitor', N'IdNumberMasked') IS NULL
         THEN '*** MISSING *** (run 003_add_idnumber_masked.sql)' ELSE 'ok' END AS Status;

/* Row counts - masters must be seeded before the tablet's host search works. */
SELECT 'vms.DiEntity' AS TableName, COUNT(*) AS Rows FROM vms.DiEntity
UNION ALL SELECT 'vms.Employee', COUNT(*) FROM vms.Employee
UNION ALL SELECT 'vms.Building', COUNT(*) FROM vms.Building
UNION ALL SELECT 'vms.Visitor',  COUNT(*) FROM vms.Visitor
UNION ALL SELECT 'vms.Visit',    COUNT(*) FROM vms.Visit;
