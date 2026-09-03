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
            "Drop the tables that are there, or switch to EF migrations, then start again.");
    }
}
