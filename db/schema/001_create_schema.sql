/* =============================================================================
   Dubai Investments - Visitor Management System
   Baseline schema (Phase 1). Target: SQL Server 2019+.
   Derived from BRD section 17, with the privacy controls of section 22 applied.

   Re-runnable: every object is guarded, so applying this to a database that is
   already partly built is safe and makes no changes to existing objects.
   ============================================================================= */

IF SCHEMA_ID(N'vms') IS NULL
    EXEC(N'CREATE SCHEMA vms AUTHORIZATION dbo;');
GO

/* ---------------------------------------------------------------- Master data */

IF OBJECT_ID(N'vms.DiEntity', N'U') IS NULL
CREATE TABLE vms.DiEntity
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DiEntity PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    EntityCode       NVARCHAR(20)     NOT NULL,
    EntityName       NVARCHAR(200)    NOT NULL,
    IsActive         BIT              NOT NULL CONSTRAINT DF_DiEntity_IsActive DEFAULT 1,
    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_DiEntity_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL,
    CONSTRAINT UQ_DiEntity_Code UNIQUE (EntityCode)
);
GO

IF OBJECT_ID(N'vms.Building', N'U') IS NULL
CREATE TABLE vms.Building
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Building PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    BuildingName     NVARCHAR(200)    NOT NULL,
    Location         NVARCHAR(200)    NULL,
    IsActive         BIT              NOT NULL CONSTRAINT DF_Building_IsActive DEFAULT 1,
    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Building_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL
);
GO

IF OBJECT_ID(N'vms.Floor', N'U') IS NULL
CREATE TABLE vms.Floor
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Floor PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    BuildingId       UNIQUEIDENTIFIER NOT NULL,
    FloorNo          NVARCHAR(10)     NOT NULL,
    FloorName        NVARCHAR(100)    NULL,
    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Floor_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Floor_Building FOREIGN KEY (BuildingId) REFERENCES vms.Building (Id),
    CONSTRAINT UQ_Floor_Building_No UNIQUE (BuildingId, FloorNo)
);
GO

IF OBJECT_ID(N'vms.Office', N'U') IS NULL
CREATE TABLE vms.Office
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Office PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    FloorId          UNIQUEIDENTIFIER NOT NULL,
    OfficeNo         NVARCHAR(20)     NOT NULL,
    Department       NVARCHAR(100)    NULL,
    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Office_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Office_Floor FOREIGN KEY (FloorId) REFERENCES vms.Floor (Id),
    CONSTRAINT UQ_Office_Floor_No UNIQUE (FloorId, OfficeNo)
);
GO

IF OBJECT_ID(N'vms.Employee', N'U') IS NULL
CREATE TABLE vms.Employee
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Employee PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    EmployeeCode     NVARCHAR(30)     NOT NULL,
    Name             NVARCHAR(200)    NOT NULL,
    DiEntityId       UNIQUEIDENTIFIER NOT NULL,
    Department       NVARCHAR(100)    NULL,
    Designation      NVARCHAR(100)    NULL,
    FloorId          UNIQUEIDENTIFIER NULL,
    OfficeId         UNIQUEIDENTIFIER NULL,
    Email            NVARCHAR(256)    NULL,
    Mobile           NVARCHAR(30)     NULL,
    IsActive         BIT              NOT NULL CONSTRAINT DF_Employee_IsActive DEFAULT 1,
    CreatedAtUtc     DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_Employee_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc    DATETIMEOFFSET(3) NULL,
    ModifiedByUserId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Employee_DiEntity FOREIGN KEY (DiEntityId) REFERENCES vms.DiEntity (Id),
    CONSTRAINT FK_Employee_Floor    FOREIGN KEY (FloorId)    REFERENCES vms.Floor (Id),
    CONSTRAINT FK_Employee_Office   FOREIGN KEY (OfficeId)   REFERENCES vms.Office (Id),
    CONSTRAINT UQ_Employee_Code UNIQUE (EmployeeCode)
);
GO

/* Host search (BRD 5) is the hottest master-data query on the tablet. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_Name_Active' AND object_id = OBJECT_ID(N'vms.Employee'))
CREATE INDEX IX_Employee_Name_Active ON vms.Employee (Name) INCLUDE (DiEntityId, Department, FloorId, OfficeId) WHERE IsActive = 1;
GO

IF OBJECT_ID(N'vms.[User]', N'U') IS NULL
CREATE TABLE vms.[User]
(
    Id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_User PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Username          NVARCHAR(128)    NOT NULL,
    Name              NVARCHAR(200)    NOT NULL,
    Role              TINYINT          NOT NULL,
    ExternalObjectId  NVARCHAR(64)     NULL,
    SecurityLocation  NVARCHAR(100)    NULL,
    IsActive          BIT              NOT NULL CONSTRAINT DF_User_IsActive DEFAULT 1,
    CanViewUnmaskedId BIT              NOT NULL CONSTRAINT DF_User_Unmasked DEFAULT 0,
    CreatedAtUtc      DATETIMEOFFSET(3) NOT NULL CONSTRAINT DF_User_Created DEFAULT SYSUTCDATETIME(),
    CreatedByUserId   UNIQUEIDENTIFIER NOT NULL,
    ModifiedAtUtc     DATETIMEOFFSET(3) NULL,
    ModifiedByUserId  UNIQUEIDENTIFIER NULL,
    CONSTRAINT UQ_User_Username UNIQUE (Username),
    CONSTRAINT CK_User_Role CHECK (Role IN (1, 2, 3, 4))
);
GO
