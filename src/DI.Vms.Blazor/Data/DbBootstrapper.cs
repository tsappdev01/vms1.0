using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DI.Vms.Blazor.Data;

/// <summary>
/// Creates this application's tables if they are not there yet.
///
/// Deliberately not <c>EnsureCreated</c>, which was here first and does not do this job:
/// it creates the schema only when it creates the database, and does nothing at all
/// against a database that already exists - not even for tables that are missing from it.
/// VMS on UATWEB01 exists and already holds ten tables from an earlier design, so
/// EnsureCreated returned false and created nothing, and the first query failed with
/// "Invalid object name 'vms.Entity'".
///
/// The DDL is generated from the EF model rather than written out here, so there is only
/// one definition of the schema and no second copy to drift.
///
/// This is still a bootstrap, not a migration tool: it can create tables that are absent,
/// never alter ones that are present. Once the database holds data that cannot be dropped,
/// move to EF migrations - <c>dotnet ef migrations add</c> then <c>Database.Migrate()</c> -
/// and delete this.
/// </summary>
public static class DbBootstrapper
{
    /// <summary>
    /// Runs the schema check, tolerating a database that is briefly unreachable at startup.
    ///
    /// EF's own retry strategy covers the errors Azure SQL raises when it throttles or moves
    /// a database between nodes. It does not cover all of them: a connection that is
    /// established and then reset during the TLS login arrives as a raw socket error inside
    /// a SqlException, which the strategy does not recognise as transient, so it gives up at
    /// once. That has happened twice on this deployment, and each time it took the whole
    /// application down at boot - App Service restarted it and the next attempt succeeded,
    /// which is recovery by luck rather than by design.
    ///
    /// So the check itself is retried. Deliberately not forever and deliberately not
    /// silently: a database that is still unreachable after a minute is a real problem, and
    /// the application should still refuse to start rather than serve a desk it cannot
    /// record anything for.
    /// </summary>
    public static async Task EnsureSchemaWithRetryAsync(
        IDbContextFactory<VmsDbContext> factory,
        ILogger logger,
        int attempts = 5,
        CancellationToken ct = default)
    {
        var delay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var db = await factory.CreateDbContextAsync(ct);
                await EnsureSchemaAsync(db, logger, ct);
                return;
            }
            catch (Exception ex) when (ex is not InvalidOperationException && attempt < attempts)
            {
                /* InvalidOperationException is excluded on purpose: those are this
                   bootstrapper's own refusals - a missing table, a missing column - and they
                   are facts about the database rather than weather. Retrying one only delays
                   a message that already says exactly which script to run. */
                logger.LogWarning(ex,
                    "Could not reach the database to check the schema (attempt {Attempt} of {Attempts}). Retrying in {Delay}.",
                    attempt, attempts, delay);

                await Task.Delay(delay, ct);
            }
        }
    }

    public static async Task EnsureSchemaAsync(VmsDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var creator = db.GetService<IRelationalDatabaseCreator>();

        if (!await creator.ExistsAsync(ct))
        {
            logger.LogInformation("Database does not exist. Creating it with the full schema.");
            await creator.CreateAsync(ct);
            await creator.CreateTablesAsync(ct);
            return;
        }

        var wanted = db.Model.GetEntityTypes()
            .Select(t => new
            {
                Schema = t.GetSchema() ?? db.Model.GetDefaultSchema() ?? "dbo",
                Table = t.GetTableName(),
            })
            .Where(t => t.Table is not null)
            .Select(t => $"{t.Schema}.{t.Table}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // One round trip, then compared in memory: simpler to read than a query per table,
        // and the model has two of them.
        var present = await db.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name AS Value FROM sys.tables t " +
                "JOIN sys.schemas s ON s.schema_id = t.schema_id")
            .ToListAsync(ct);

        var found = new HashSet<string>(present, StringComparer.OrdinalIgnoreCase);
        var missing = wanted.Where(t => !found.Contains(t)).ToList();

        if (missing.Count == 0)
        {
            logger.LogInformation("Schema present: {Tables}.", string.Join(", ", wanted));
            await VerifyColumnsAsync(db, logger, ct);
            return;
        }

        if (missing.Count == wanted.Count)
        {
            logger.LogInformation("Creating {Tables}.", string.Join(", ", missing));

            /* Generated from the model, so it carries the indexes and the foreign key too.
               The CREATE SCHEMA it emits is guarded by SCHEMA_ID, so an existing "vms"
               schema - which this database has - is left alone. */
            await creator.CreateTablesAsync(ct);
            return;
        }

        /* Some but not all: creating the rest would need DDL for exactly the missing
           subset, and guessing whether the tables that do exist match the model is how a
           database quietly stops matching the code. Say what is wrong and stop, rather
           than start an application whose reports would be wrong. */
        throw new InvalidOperationException(
            $"The database is missing {string.Join(", ", missing)} but already has " +
            $"{string.Join(", ", wanted.Except(missing, StringComparer.OrdinalIgnoreCase))}. " +
            "Run the scripts in db/ in number order against this database - the one that " +
            "adds the missing table is there - and start again.");
    }

    /// <summary>
    /// Checks that every column the model expects exists on the tables that are already
    /// there, and says which are missing if any are.
    ///
    /// This bootstrapper creates absent tables but never alters present ones, so a
    /// property added to the model reaches an existing database only through a script in
    /// db/. Without this check the first query fails instead, with SQL Server's
    /// "Invalid column name" and no indication of which script to run - the same
    /// confusion that the missing tables caused.
    /// </summary>
    private static async Task VerifyColumnsAsync(VmsDbContext db, ILogger logger, CancellationToken ct)
    {
        var present = await db.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name + '.' + c.name AS Value FROM sys.columns c " +
                "JOIN sys.tables t ON t.object_id = c.object_id " +
                "JOIN sys.schemas s ON s.schema_id = t.schema_id")
            .ToListAsync(ct);

        var found = new HashSet<string>(present, StringComparer.OrdinalIgnoreCase);
        var missing = new List<string>();

        foreach (var type in db.Model.GetEntityTypes())
        {
            var table = type.GetTableName();
            if (table is null) continue;

            var schema = type.GetSchema() ?? db.Model.GetDefaultSchema() ?? "dbo";

            foreach (var property in type.GetProperties())
            {
                var column = property.GetColumnName();
                if (string.IsNullOrEmpty(column)) continue;
                if (!found.Contains($"{schema}.{table}.{column}")) missing.Add($"{table}.{column}");
            }
        }

        if (missing.Count == 0) return;

        throw new InvalidOperationException(
            $"The database is missing {missing.Count} column(s) the code expects: " +
            $"{string.Join(", ", missing)}. Run the scripts in db/ against this database - " +
            "the newest one adds them - and start again.");
    }
}
