namespace DI.Vms.Domain.ValueObjects;

/// <summary>
/// A home address read from the Emirates ID chip. Owned by <c>Visitor</c> and stored as
/// discrete columns, so the retention job can clear it and a report can render it
/// without parsing.
/// </summary>
public sealed class VisitorAddress
{
    public string? Emirate { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public string? Street { get; set; }
    public string? Building { get; set; }
    public string? Flat { get; set; }
    public string? PoBox { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }

    /// <summary>True when nothing was captured, so callers can avoid storing an empty shell.</summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Emirate) && string.IsNullOrWhiteSpace(City) &&
        string.IsNullOrWhiteSpace(Area) && string.IsNullOrWhiteSpace(Street) &&
        string.IsNullOrWhiteSpace(Building) && string.IsNullOrWhiteSpace(Flat) &&
        string.IsNullOrWhiteSpace(PoBox) && string.IsNullOrWhiteSpace(Mobile) &&
        string.IsNullOrWhiteSpace(Email);

    /// <summary>One readable line, for reports and screens.</summary>
    public override string ToString()
    {
        var parts = new[] { Flat, Building, Street, Area, City, Emirate }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }
}
