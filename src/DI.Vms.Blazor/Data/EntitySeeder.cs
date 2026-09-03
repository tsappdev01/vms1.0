using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Blazor.Data;

/// <summary>
/// Makes the entity table match <see cref="Names"/>, on every startup, so the dropdown on
/// the New Visitor screen is driven entirely from the database.
///
/// Deliberately not EF's HasData: that only ever runs when the table is created, so a
/// change to the list would never reach a database that already holds one.
///
/// Entities no longer listed are retired rather than deleted: visitor entries reference
/// them, and a report covering last month must still be able to name the entity that was
/// visited. Retiring takes them out of the dropdown, which is what "removed" means here.
/// </summary>
public static class EntitySeeder
{
    /// <summary>
    /// The DI group companies, as they appear in the existing employee-company dropdown.
    ///
    /// Transcribed from a screenshot of that dropdown, which was cut off after
    /// "TechSource" - so anything sorting after T may be missing. Add it here and restart.
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
        var existing = await db.DiEntities.ToListAsync(ct);

        /* Case-insensitive throughout, to match the unique index: SQL Server's default
           collation already treats "DI" and "di" as the same name, so comparing any other
           way would try to insert a duplicate the database then rejects. */
        var byName = existing
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var wanted = new HashSet<string>(Names, StringComparer.OrdinalIgnoreCase);

        List<string> added = [], restored = [], retired = [];

        foreach (var name in Names)
        {
            if (byName.TryGetValue(name, out var entity))
            {
                // Listed again after being retired, or after someone deactivated it by hand.
                if (!entity.IsActive)
                {
                    entity.IsActive = true;
                    restored.Add(entity.Name);
                }
            }
            else
            {
                db.DiEntities.Add(new DiEntity { Name = name });
                added.Add(name);
            }
        }

        foreach (var entity in existing.Where(e => e.IsActive && !wanted.Contains(e.Name)).ToList())
        {
            entity.IsActive = false;
            retired.Add(entity.Name);
        }

        if (added.Count == 0 && restored.Count == 0 && retired.Count == 0)
        {
            logger.LogInformation("Entity list already in sync ({Count} entities).", existing.Count);
            return;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Entity list synced: added [{Added}], restored [{Restored}], retired [{Retired}].",
            string.Join(", ", added), string.Join(", ", restored), string.Join(", ", retired));
    }
}
