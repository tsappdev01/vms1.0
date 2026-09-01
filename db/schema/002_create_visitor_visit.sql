/* =============================================================================
   Visitor, Visit, Signature and Audit tables.
   ID numbers are encrypted at rest and looked up by keyed hash (BRD 22).

   Re-runnable: every object is guarded, so applying this to a database that is
   already partly built is safe and makes no changes to existing objects.
   ============================================================================= */

IF OBJECT_ID(N'vms.Visitor', N'U') IS NULL
CREATE TABLE vms.Visitor
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Visitor PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Name             NVARCHAR(200)    NOT NULL,
    Company          NVARCHAR(200)    NULL,
    IdType           TINYINT          NOT NULL,

    /* Ciphertext of the ID number. Encrypted by the application (envelope encryption,
       key in Azure Key Vault / SQL AKV provider) - never stored in clear. */
    IdNumberCipher   VARBINARY(512)   NOT NULL,

    /* HMAC-SHA256 of the normalised ID number under a server-side pepper.
       Enables repeat-visitor lookup (BRD 14) without decrypting the table. */
    IdNumberHash     BINARY(32)       NOT NULL,

    IdExpiryDate     DATE             NULL,
    Nationality      NVARCHAR(100)    NULL,
    DateOfBirth      DATE             NULL,
    Photo            VARBINARY(MAX)   NULL,
    CaptureMethod    TINYINT          NOT NULL CONSTRAINT DF_Visitor_Capture DEFAULT 0,
    PurgedAtUtc      DATETIMEOFFSET(3) NULL,

    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Visitor_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL,

    CONSTRAINT UQ_Visitor_IdHash UNIQUE (IdType, IdNumberHash)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visitor_Name' AND object_id = OBJECT_ID(N'vms.Visitor'))
CREATE INDEX IX_Visitor_Name ON vms.Visitor (Name) INCLUDE (Company);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visitor_Company' AND object_id = OBJECT_ID(N'vms.Visitor'))
CREATE INDEX IX_Visitor_Company ON vms.Visitor (Company) WHERE Company IS NOT NULL;
GO
/* Expired ID Report (BRD 20). */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visitor_IdExpiry' AND object_id = OBJECT_ID(N'vms.Visitor'))
CREATE INDEX IX_Visitor_IdExpiry ON vms.Visitor (IdExpiryDate) WHERE IdExpiryDate IS NOT NULL;
GO

IF OBJECT_ID(N'vms.Visit', N'U') IS NULL
CREATE TABLE vms.Visit
(
    Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Visit PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    VisitNumber          NVARCHAR(20)     NOT NULL,
    VisitorId            UNIQUEIDENTIFIER NOT NULL,
    DiEntityId           UNIQUEIDENTIFIER NOT NULL,
    HostEmployeeId       UNIQUEIDENTIFIER NOT NULL,
    Purpose              NVARCHAR(400)    NULL,
    VisitType            TINYINT          NOT NULL CONSTRAINT DF_Visit_Type DEFAULT 1,

    /* Snapshotted from the host at registration so a later desk move does not rewrite history. */
    [Floor]              NVARCHAR(10)     NULL,
    Office               NVARCHAR(20)     NULL,
    Department           NVARCHAR(100)    NULL,

    ExpectedDate         DATE             NULL,
    ExpectedTime         TIME(0)          NULL,
    InTimeUtc            DATETIMEOFFSET(3) NULL,
    OutTimeUtc           DATETIMEOFFSET(3) NULL,
    Status               TINYINT          NOT NULL CONSTRAINT DF_Visit_Status DEFAULT 1,

    CheckedInOnDeviceId  NVARCHAR(100)    NULL,
    CheckedInByUserId    UNIQUEIDENTIFIER NULL,
    CheckedOutByUserId   UNIQUEIDENTIFIER NULL,
    AutoClosed           BIT              NOT NULL CONSTRAINT DF_Visit_AutoClosed DEFAULT 0,

    CreatedAtUtc         DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Visit_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId      UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc        DATETIMEOFFSET(3) NULL,
    ModifiedByUserId     UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_Visit_Visitor  FOREIGN KEY (VisitorId)      REFERENCES vms.Visitor (Id),
    CONSTRAINT FK_Visit_DiEntity FOREIGN KEY (DiEntityId)     REFERENCES vms.DiEntity (Id),
    CONSTRAINT FK_Visit_Host     FOREIGN KEY (HostEmployeeId) REFERENCES vms.Employee (Id),
    CONSTRAINT UQ_Visit_Number UNIQUE (VisitNumber),
    CONSTRAINT CK_Visit_Status CHECK (Status IN (1, 2, 3, 4)),
    /* A visit that is Inside must have an in-time; one that is CheckedOut must have both. */
    CONSTRAINT CK_Visit_Times CHECK
    (
        (Status = 1 AND InTimeUtc IS NULL AND OutTimeUtc IS NULL)
        OR (Status = 2 AND InTimeUtc IS NOT NULL AND OutTimeUtc IS NULL)
        OR (Status = 3 AND InTimeUtc IS NOT NULL AND OutTimeUtc IS NOT NULL AND OutTimeUtc >= InTimeUtc)
        OR (Status = 4)
    )
);
GO

/* The dashboard's "currently inside" query (BRD 11) and the evacuation list (BRD 12). */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visit_Inside' AND object_id = OBJECT_ID(N'vms.Visit'))
CREATE INDEX IX_Visit_Inside ON vms.Visit (InTimeUtc) INCLUDE (VisitorId, HostEmployeeId, DiEntityId, [Floor]) WHERE Status = 2;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visit_Visitor_InTime' AND object_id = OBJECT_ID(N'vms.Visit'))
CREATE INDEX IX_Visit_Visitor_InTime ON vms.Visit (VisitorId, InTimeUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visit_Host_InTime' AND object_id = OBJECT_ID(N'vms.Visit'))
CREATE INDEX IX_Visit_Host_InTime ON vms.Visit (HostEmployeeId, InTimeUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Visit_Expected' AND object_id = OBJECT_ID(N'vms.Visit'))
CREATE INDEX IX_Visit_Expected ON vms.Visit (ExpectedDate) WHERE Status = 1;
GO

IF OBJECT_ID(N'vms.VisitorSignature', N'U') IS NULL
CREATE TABLE vms.VisitorSignature
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VisitorSignature PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    VisitId       UNIQUEIDENTIFIER NOT NULL,
    Image         VARBINARY(MAX)   NOT NULL,
    CapturedAtUtc DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Sig_Captured DEFAULT SYSUTCDATETIME(),
    DeviceId      NVARCHAR(100)    NULL,
    CONSTRAINT FK_Signature_Visit FOREIGN KEY (VisitId) REFERENCES vms.Visit (Id),
    CONSTRAINT UQ_Signature_Visit UNIQUE (VisitId)
);
GO

/* Append-only. Grant INSERT and SELECT only; no UPDATE or DELETE outside the retention job. */
IF OBJECT_ID(N'vms.AuditLog', N'U') IS NULL
CREATE TABLE vms.AuditLog
(
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    Action       NVARCHAR(60)     NOT NULL,
    EntityName   NVARCHAR(60)     NOT NULL,
    RecordId     UNIQUEIDENTIFIER NULL,
    OldValue     NVARCHAR(MAX)    NULL,
    NewValue     NVARCHAR(MAX)    NULL,
    TimestampUtc DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Audit_Ts DEFAULT SYSUTCDATETIME(),
    IpAddress    NVARCHAR(45)     NULL,
    DeviceId     NVARCHAR(100)    NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_Ts' AND object_id = OBJECT_ID(N'vms.AuditLog'))
CREATE INDEX IX_AuditLog_Ts ON vms.AuditLog (TimestampUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_Record' AND object_id = OBJECT_ID(N'vms.AuditLog'))
CREATE INDEX IX_AuditLog_Record ON vms.AuditLog (EntityName, RecordId, TimestampUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_User' AND object_id = OBJECT_ID(N'vms.AuditLog'))
CREATE INDEX IX_AuditLog_User ON vms.AuditLog (UserId, TimestampUtc DESC);
GO

/* Gap-free, year-scoped visit numbers: VIS-2026-00001245 (BRD 7). */
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'VisitNumberSequence' AND schema_id = SCHEMA_ID(N'vms'))
CREATE SEQUENCE vms.VisitNumberSequence AS INT START WITH 1 INCREMENT BY 1;
GO

CREATE OR ALTER FUNCTION vms.NextVisitNumber(@year INT, @seq INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    RETURN CONCAT(N'VIS-', @year, N'-', RIGHT(CONCAT(N'00000000', @seq), 8));
END;
GO
