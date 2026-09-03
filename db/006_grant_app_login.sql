/*  006_grant_app_login.sql
    Gives the VMS application's Windows identity access to the VMS database.

    Run on UATWEB01 as an administrator, once, when deploying to a new reception PC.

    Set @Login below to the identity the app runs as:

      DI\svc-vms      a domain service account - the simplest to grant and to audit
      DI\RECEPTION1$  the reception PC's machine account, if the service runs as
                      LocalSystem or NetworkService; note the trailing $

    Re-runnable: it creates only what is absent and adds only the roles that are missing.

    Why db_datareader + db_datawriter and not db_owner: the application reads and writes
    rows. It does not create tables in a deployed database - Data/DbBootstrapper.cs
    creates absent tables from the EF model, which is right for a fresh database and for
    development, but on a live one the schema should arrive by script, reviewed, the way
    everything in this folder does. If the bootstrapper needs to create a table here it
    will fail with a permission error, and that failure is the intended signal that a
    migration script is missing - not a reason to grant db_owner.
*/

USE VMS;
GO

SET NOCOUNT ON;

DECLARE @Login sysname = N'DI\svc-vms';   -- <<< EDIT THIS
DECLARE @sql nvarchar(max);

/* 1. Server login. CREATE LOGIN is a server-level statement and runs from any database
      context, so this does not need to switch to master. */
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @Login)
BEGIN
    SET @sql = N'CREATE LOGIN ' + QUOTENAME(@Login) + N' FROM WINDOWS';
    EXEC sys.sp_executesql @sql;
    PRINT 'Created login ' + @Login + '.';
END
ELSE
    PRINT 'Login ' + @Login + ' already exists.';

/* 2. Database user in VMS. */
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @Login)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@Login) + N' FOR LOGIN ' + QUOTENAME(@Login);
    EXEC sys.sp_executesql @sql;
    PRINT 'Created database user ' + @Login + ' in VMS.';
END
ELSE
    PRINT 'Database user ' + @Login + ' already exists in VMS.';

/* 3. Roles. */
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datareader' AND u.name = @Login)
BEGIN
    SET @sql = N'ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@Login);
    EXEC sys.sp_executesql @sql;
    PRINT 'Added ' + @Login + ' to db_datareader.';
END
ELSE
    PRINT @Login + ' is already in db_datareader.';

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
    WHERE r.name = N'db_datawriter' AND u.name = @Login)
BEGIN
    SET @sql = N'ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@Login);
    EXEC sys.sp_executesql @sql;
    PRINT 'Added ' + @Login + ' to db_datawriter.';
END
ELSE
    PRINT @Login + ' is already in db_datawriter.';

/* 4. The app reads and writes the vms schema only. Granting on the schema as well as
      through the roles keeps a future table outside vms out of reach. */
IF SCHEMA_ID(N'vms') IS NOT NULL
BEGIN
    SET @sql = N'GRANT SELECT, INSERT, UPDATE ON SCHEMA::vms TO ' + QUOTENAME(@Login);
    EXEC sys.sp_executesql @sql;
    PRINT 'Granted SELECT, INSERT, UPDATE on schema vms.';
END
ELSE
    PRINT 'Schema vms does not exist yet - run 001_seed_entities.sql first, then re-run this.';
GO
