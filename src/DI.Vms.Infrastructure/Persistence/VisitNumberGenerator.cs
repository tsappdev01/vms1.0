using DI.Vms.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DI.Vms.Infrastructure.Persistence;

/// <summary>
/// Allocates VIS-{year}-{8 digits} from the database sequence (BRD 7). The sequence keeps
/// allocation safe across concurrent reception desks; doing it with MAX(VisitNumber) + 1
/// would hand two tablets the same number.
/// </summary>
public sealed class VisitNumberGenerator(VmsDbContext db) : IVisitNumberGenerator
{
    public async Task<string> NextAsync(CancellationToken ct = default)
    {
        var next = await db.Database
            .SqlQuery<int>($"SELECT CAST(NEXT VALUE FOR vms.VisitNumberSequence AS int) AS Value")
            .SingleAsync(ct);

        return $"VIS-{DateTime.UtcNow.Year}-{next:D8}";
    }
}
