/*  008_grant_app_user_azure.sql
    Gives the Azure Web App access to the VMS database on Azure SQL.

    The Azure counterpart of 006_grant_app_login.sql, which cannot be used here: it does
    CREATE LOGIN ... FROM WINDOWS, and Azure SQL has neither Windows authentication nor
    server-level logins of that shape. Azure SQL uses contained database users instead -
    the principal lives in the database, not on the server.

    Run it connected to the VMS database on Azure SQL, as an administrator. For the
    managed-identity option below that administrator must itself be signed in with Entra
    ID, because only an Entra principal can create one.

    -------------------------------------------------------------------------------
    BEFORE THIS: the schema has to exist

    There is no schema script in this folder. Data/DbBootstrapper.cs creates the tables
    from the EF model at startup, which is right for a fresh database - and a brand new
    Azure SQL database is exactly that. But the user this script creates is deliberately
    NOT allowed to create tables (same reasoning as 006: if the bootstrapper has to create
    a table in a live database, that is a missing migration script and should fail).

    So, in order:

      1. Point the Web App's ConnectionStrings__Vms at the Azure SQL ADMIN account and
         start it once. DbBootstrapper creates vms.VisitorEntry, vms.Entity and the rest,
         and the log says what it made.
      2. Run 001, 002, 003, 004, 005 and 007 against the database - the reference data and
         the columns added since.
      3. Run this script.
      4. Change ConnectionStrings__Vms to the least-privileged principal below, and
         restart. The admin credential should not be what the app runs as.

    -------------------------------------------------------------------------------
    Re-runnable: creates only what is absent, adds only the roles that are missing.
*/

/*  No USE: Azure SQL does not support it. Connect to VMS directly. This script is
    Azure-only, so the guard checks for the database rather than for master. */
IF DB_NAME() IN (N'master', N'msdb', N'model', N'tempdb')
BEGIN
    RAISERROR('Connect to the VMS database before running this script. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

SET NOCOUNT ON;
GO

/*  ---------------------------------------------------------------------------------
    OPTION A - the Web App's managed identity.   RECOMMENDED.

    No password exists, so none can be leaked, committed, or left in App Service
    configuration. Azure hands the app a token at runtime and rotates the credential
    itself.

    @User is the Web App's name, which is what its system-assigned identity is called.
    Enable it first: Web App > Settings > Identity > System assigned > On.

    The connection string then carries no secret at all:

        Server=tcp:<server>.database.windows.net,1433;Database=VMS;
        Authentication=Active Directory Default;Encrypt=True;

    Uncomment the block and set @User.
    --------------------------------------------------------------------------------- */

/*
DECLARE @User sysname = N'vms-api';          -- <<< the Web App's name
DECLARE @sql  nvarchar(max);

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @User)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@User) + N' FROM EXTERNAL PROVIDER';
    EXEC sys.sp_executesql @sql;
    PRINT 'Created contained user ' + @User + ' from Entra ID.';
END
ELSE
    PRINT 'User ' + @User + ' already exists.';

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datareader' AND u.name = @User)
BEGIN
    SET @sql = N'ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@User);
    EXEC sys.sp_executesql @sql;
    PRINT 'Added ' + @User + ' to db_datareader.';
END
ELSE
    PRINT @User + ' is already in db_datareader.';

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datawriter' AND u.name = @User)
BEGIN
    SET @sql = N'ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@User);
    EXEC sys.sp_executesql @sql;
    PRINT 'Added ' + @User + ' to db_datawriter.';
END
ELSE
    PRINT @User + ' is already in db_datawriter.';
*/
GO

/*  ---------------------------------------------------------------------------------
    OPTION B - a contained user with a password.

    Use only where managed identity is not available. The password then lives in App
    Service configuration and in whoever's hands it passed through on the way, and it
    does not rotate on its own. Put its expiry in a calendar.

    Connection string:

        Server=tcp:<server>.database.windows.net,1433;Database=VMS;
        User ID=vms_app;Password=<the password>;Encrypt=True;

    Uncomment and set both values.
    --------------------------------------------------------------------------------- */

/*
DECLARE @User2 sysname        = N'vms_app';
DECLARE @Password nvarchar(128) = N'<a long random password>';   -- <<< EDIT
DECLARE @sql2 nvarchar(max);

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @User2)
BEGIN
    SET @sql2 = N'CREATE USER ' + QUOTENAME(@User2) + N' WITH PASSWORD = ' + QUOTENAME(@Password, '''');
    EXEC sys.sp_executesql @sql2;
    PRINT 'Created contained user ' + @User2 + '.';
END
ELSE
    PRINT 'User ' + @User2 + ' already exists.';

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datareader' AND u.name = @User2)
BEGIN
    SET @sql2 = N'ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@User2);
    EXEC sys.sp_executesql @sql2;
    PRINT 'Added ' + @User2 + ' to db_datareader.';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datawriter' AND u.name = @User2)
BEGIN
    SET @sql2 = N'ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@User2);
    EXEC sys.sp_executesql @sql2;
    PRINT 'Added ' + @User2 + ' to db_datawriter.';
END
*/
GO

/* What ended up with access. Run this on its own afterwards to check. */
SELECT p.name AS Principal, p.type_desc AS Kind, r.name AS Role
FROM sys.database_role_members m
JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
JOIN sys.database_principals p ON p.principal_id = m.member_principal_id
WHERE p.name NOT IN (N'dbo', N'public')
ORDER BY p.name, r.name;
GO

SET NOEXEC OFF;
GO
