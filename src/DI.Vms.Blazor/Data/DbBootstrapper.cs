using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;

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
/// It creates absent tables, adds absent <em>nullable</em> columns to tables that are
/// already there, and creates absent indexes. Those three are deliberately the whole of
/// what it will alter: none of them can lose data, and each is what an ordinary addition to
/// the model needs. Everything else - a required column, a widened type, a renamed or
/// dropped column - still needs a script in db/, because those have a right answer only a
/// person knows.
///
/// This is still a bootstrap, not a migration tool. Once the database holds data that
/// cannot be dropped and the changes stop being additive, move to EF migrations -
/// <c>dotnet ef migrations add</c> then <c>Database.Migrate()</c> - and delete this.
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
            await EnsureIndexesAsync(db, logger, ct);
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
    /// Brings the columns on existing tables up to what the model expects.
    ///
    /// A nullable column that the model has and the database lacks is added here, because
    /// a new optional field is the ordinary change and making every one of them a manual
    /// SQL step means a deployment that is two things instead of one - and the second gets
    /// forgotten, at which point the first query fails with SQL Server's "Invalid column
    /// name" and no indication of what to run.
    ///
    /// Anything it cannot add safely still stops the application, naming the columns. A
    /// required column added to a table with rows in it needs a default that only a person
    /// can choose, and guessing one is how a database quietly stops meaning what the
    /// reports say it means.
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

        var addable = new List<(string Schema, string Table, string Column, string Type)>();
        var refused = new List<string>();

        var sql = db.GetService<ISqlGenerationHelper>();

        foreach (var type in db.Model.GetEntityTypes())
        {
            var table = type.GetTableName();
            if (table is null) continue;

            var schema = type.GetSchema() ?? db.Model.GetDefaultSchema() ?? "dbo";

            foreach (var property in type.GetProperties())
            {
                var column = property.GetColumnName();
                if (string.IsNullOrEmpty(column)) continue;
                if (found.Contains($"{schema}.{table}.{column}")) continue;

                if (property.IsNullable)
                {
                    addable.Add((schema, table, column, property.GetColumnType()));
                }
                else
                {
                    /* A required column on a table that already has rows needs a value for
                       every one of them. What that value should be is a decision, not a
                       default. */
                    refused.Add($"{table}.{column}");
                }
            }
        }

        foreach (var (schema, table, column, columnType) in addable)
        {
            /* IF NOT EXISTS as well as the check above, because two instances of this app
               start together on App Service and both would otherwise run the same ALTER. */
            var statement =
                $"IF COL_LENGTH('{schema}.{table}', '{column}') IS NULL " +
                $"ALTER TABLE {sql.DelimitIdentifier(table, schema)} " +
                $"ADD {sql.DelimitIdentifier(column)} {columnType} NULL;";

            await db.Database.ExecuteSqlRawAsync(statement, ct);

            logger.LogInformation(
                "Added the column {Schema}.{Table}.{Column} ({Type}), which the model has and the database did not.",
                schema, table, column, columnType);
        }

        if (refused.Count == 0) return;

        throw new InvalidOperationException(
            $"The database is missing {refused.Count} required column(s) the code expects: " +
            $"{string.Join(", ", refused)}. A required column cannot be added to a table " +
            "with rows in it without deciding what the existing rows should say, so this " +
            "one is not automatic. Run the scripts in db/ against this database - the " +
            "newest one adds them - and start again.");
    }

    /// <summary>
    /// Creates the indexes the model declares and the database does not have.
    ///
    /// This used to be a script somebody had to run, on the reasoning that CREATE INDEX
    /// takes a schema lock and when to take one is a decision. That reasoning is sound and
    /// was the wrong trade here: a reception desk's visit table is thousands of rows, where
    /// the lock is measured in milliseconds, and the real cost of the manual step was that
    /// it got skipped - leaving a screen that scans a table to answer a question it has an
    /// index for, with nobody knowing why it felt slow.
    ///
    /// So it is automatic, with the guard that makes it stay safe: above
    /// <see cref="IndexRowCeiling"/> rows it refuses and names the script instead. The point
    /// at which the original objection becomes real is the point at which a person should
    /// choose the moment.
    /// </summary>
    private const long IndexRowCeiling = 500_000;

    private static async Task EnsureIndexesAsync(VmsDbContext db, ILogger logger, CancellationToken ct)
    {
        var present = await db.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name + '.' + i.name AS Value FROM sys.indexes i " +
                "JOIN sys.tables t ON t.object_id = i.object_id " +
                "JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                "WHERE i.name IS NOT NULL")
            .ToListAsync(ct);

        var found = new HashSet<string>(present, StringComparer.OrdinalIgnoreCase);
        var sql = db.GetService<ISqlGenerationHelper>();

        foreach (var type in db.Model.GetEntityTypes())
        {
            var table = type.GetTableName();
            if (table is null) continue;

            var schema = type.GetSchema() ?? db.Model.GetDefaultSchema() ?? "dbo";

            foreach (var index in type.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (string.IsNullOrEmpty(name)) continue;
                if (found.Contains($"{schema}.{table}.{name}")) continue;

                var columns = index.Properties
                    .Select(property => property.GetColumnName())
                    .Where(column => !string.IsNullOrEmpty(column))
                    .ToList();

                if (columns.Count != index.Properties.Count) continue;

                var rows = await RowCountAsync(db, schema, table, ct);

                if (rows > IndexRowCeiling)
                {
                    /* Not an exception: the application runs perfectly well without an
                       index, just with a scan. Refusing to start over a missing index would
                       take a working desk down to fix a slow screen. */
                    logger.LogWarning(
                        "{Schema}.{Table} has {Rows:N0} rows, so the index {Index} is not created " +
                        "automatically - CREATE INDEX would hold a schema lock for long enough to " +
                        "matter. Run the script in db/ that adds it, at a quiet moment.",
                        schema, table, rows, name);
                    continue;
                }

                var unique = index.IsUnique ? "UNIQUE " : string.Empty;
                var filter = index.GetFilter() is { Length: > 0 } f ? $" WHERE {f}" : string.Empty;
                var columnList = string.Join(", ", columns.Select(c => sql.DelimitIdentifier(c!)));

                var statement =
                    $"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = " +
                    $"OBJECT_ID('{schema}.{table}', 'U') AND name = '{name}') " +
                    $"CREATE {unique}INDEX {sql.DelimitIdentifier(name)} " +
                    $"ON {sql.DelimitIdentifier(table, schema)} ({columnList}){filter};";

                try
                {
                    await db.Database.ExecuteSqlRawAsync(statement, ct);

                    logger.LogInformation(
                        "Created the index {Schema}.{Table}.{Index} over ({Columns}){Filter}.",
                        schema, table, name, string.Join(", ", columns), filter);
                }
                catch (Exception ex)
                {
                    /* An index is a performance decision, and a database that will not take
                       one is not a reason to refuse a reception desk. Said loudly, then on. */
                    logger.LogError(ex,
                        "Could not create the index {Index} on {Schema}.{Table}. The application " +
                        "will run without it, reading that table by scan.",
                        name, schema, table);
                }
            }
        }
    }

    /// <summary>
    /// How many rows the table holds, from the partition statistics rather than COUNT(*).
    ///
    /// COUNT(*) on a table large enough for this question to matter is itself the scan the
    /// question is about.
    /// </summary>
    private static async Task<long> RowCountAsync(
        VmsDbContext db, string schema, string table, CancellationToken ct)
    {
        var rows = await db.Database
            .SqlQueryRaw<long>(
                "SELECT ISNULL(SUM(p.row_count), 0) AS Value FROM sys.dm_db_partition_stats p " +
                $"WHERE p.object_id = OBJECT_ID('{schema}.{table}', 'U') AND p.index_id IN (0, 1)")
            .ToListAsync(ct);

        return rows.FirstOrDefault();
    }
}
