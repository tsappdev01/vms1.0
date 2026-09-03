using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Blazor.Data;

/// <summary>
/// Ensures the entity list exists, on every startup.
///
/// Deliberately not EF's HasData: that seeds only when the database is created, so a
/// change to the list would never reach a database that already exists. This inserts
/// what is missing and leaves everything else alone, so it is safe to run repeatedly and
/// safe to run against a database someone has already edited.
/// </summary>
public static class EntitySeeder
{
    /// <summary>
    /// The DI group companies, as they appear in the existing employee-company dropdown.
    ///
    /// Transcribed from a screenshot of that dropdown, which was cut off after
    /// "TechSource" - so anything sorting after T may be missing. Add it here and restart;
    /// the sync is additive.
    /// </summary>
    public static readonly string[] Names =
    [
        "ALMujama",
        "DanahBay",
        "DI",
        "DII",
        "DIP",
        "DIR",
        "GlassLLC",
        "Masharie",
        "PI",
        "PIDOA",
        "TechSource",
    ];

    public static async Task SyncAsync(VmsDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var existing = await db.DiEntities
            .Select(e => e.Name)
            .ToListAsync(ct);

        // Case-insensitive, so a differently-cased duplicate is not inserted alongside.
        var known = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var missing = Names.Where(n => !known.Contains(n)).ToList();

        if (missing.Count == 0)
        {
            logger.LogInformation("Entity list already complete ({Count} entities).", existing.Count);
            return;
        }

        db.DiEntities.AddRange(missing.Select(name => new DiEntity { Name = name }));
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Added {Count} entities: {Names}", missing.Count, string.Join(", ", missing));
    }
}
