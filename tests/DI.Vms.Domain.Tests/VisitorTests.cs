using DI.Vms.Domain.Entities;
using DI.Vms.Domain.Enums;
using Xunit;

namespace DI.Vms.Domain.Tests;

public class VisitorTests
{
    private static Visitor WithExpiry(DateOnly? expiry) => new()
    {
        Name = "Ahmed Khan",
        IdType = IdType.EmiratesId,
        IdNumberCipher = [1, 2, 3],
        IdNumberHash = new byte[32],
        IdNumberMasked = "784-XXXX-XXXXXXX-1",
        IdExpiryDate = expiry,
    };

    [Fact]
    public void An_id_expiring_before_today_is_expired()
    {
        var visitor = WithExpiry(new DateOnly(2024, 1, 31));
        Assert.True(visitor.IsIdExpired(new DateOnly(2026, 9, 2)));
    }

    [Fact]
    public void An_id_expiring_in_the_future_is_not_expired()
    {
        var visitor = WithExpiry(new DateOnly(2028, 4, 15));
        Assert.False(visitor.IsIdExpired(new DateOnly(2026, 9, 2)));
    }

    /// <summary>Expiring today is still valid: the card is good until the day is out.</summary>
    [Fact]
    public void An_id_expiring_today_is_not_expired()
    {
        var today = new DateOnly(2026, 9, 2);
        Assert.False(WithExpiry(today).IsIdExpired(today));
    }

    [Fact]
    public void An_unknown_expiry_is_not_treated_as_expired()
    {
        Assert.False(WithExpiry(null).IsIdExpired(new DateOnly(2026, 9, 2)));
    }
}
