using DI.Vms.Domain.Common;
using DI.Vms.Domain.Entities;
using DI.Vms.Domain.Enums;
using Xunit;

namespace DI.Vms.Domain.Tests;

public class VisitLifecycleTests
{
    private static readonly Guid Officer = Guid.NewGuid();
    private static readonly DateTimeOffset Nine = new(2026, 9, 1, 5, 42, 0, TimeSpan.Zero);

    private static Visit NewVisit() => new()
    {
        VisitNumber = "VIS-2026-00001245",
        VisitorId = Guid.NewGuid(),
        DiEntityId = Guid.NewGuid(),
        HostEmployeeId = Guid.NewGuid(),
        Status = VisitStatus.Expected,
    };

    private static VisitorSignature Signature() => new() { Image = [1, 2, 3] };

    [Fact]
    public void Check_in_records_time_device_user_and_signature()
    {
        var visit = NewVisit();

        visit.CheckIn(Officer, "RECEPTION-TABLET-01", Signature(), Nine);

        Assert.Equal(VisitStatus.Inside, visit.Status);
        Assert.Equal(Nine, visit.InTimeUtc);
        Assert.Equal(Officer, visit.CheckedInByUserId);
        Assert.Equal("RECEPTION-TABLET-01", visit.CheckedInOnDeviceId);
        Assert.NotNull(visit.Signature);
        Assert.True(visit.IsInside);
    }

    [Fact]
    public void Check_in_twice_is_refused()
    {
        var visit = NewVisit();
        visit.CheckIn(Officer, "D1", Signature(), Nine);

        var ex = Assert.Throws<DomainException>(() =>
            visit.CheckIn(Officer, "D1", Signature(), Nine.AddMinutes(5)));

        Assert.Contains("already checked in", ex.Message);
    }

    [Fact]
    public void Check_out_records_time_and_duration()
    {
        var visit = NewVisit();
        visit.CheckIn(Officer, "D1", Signature(), Nine);

        // The BRD's worked example: in 09:42, out 11:18, duration 1h 36m.
        visit.CheckOut(Officer, Nine.AddMinutes(96));

        Assert.Equal(VisitStatus.CheckedOut, visit.Status);
        Assert.Equal(TimeSpan.FromMinutes(96), visit.Duration);
        Assert.False(visit.IsInside);
    }

    [Fact]
    public void Check_out_before_check_in_is_refused()
    {
        var visit = NewVisit();

        var ex = Assert.Throws<DomainException>(() => visit.CheckOut(Officer, Nine));

        Assert.Contains("only a visit that is Inside", ex.Message);
    }

    [Fact]
    public void Check_out_twice_is_refused()
    {
        var visit = NewVisit();
        visit.CheckIn(Officer, "D1", Signature(), Nine);
        visit.CheckOut(Officer, Nine.AddMinutes(96));

        Assert.Throws<DomainException>(() => visit.CheckOut(Officer, Nine.AddMinutes(120)));
    }

    /// <summary>A negative duration would corrupt every duration report that follows.</summary>
    [Fact]
    public void Check_out_earlier_than_check_in_is_refused()
    {
        var visit = NewVisit();
        visit.CheckIn(Officer, "D1", Signature(), Nine);

        var ex = Assert.Throws<DomainException>(() => visit.CheckOut(Officer, Nine.AddMinutes(-10)));

        Assert.Contains("cannot precede", ex.Message);
    }

    [Fact]
    public void A_checked_out_visit_cannot_be_checked_in_again()
    {
        var visit = NewVisit();
        visit.CheckIn(Officer, "D1", Signature(), Nine);
        visit.CheckOut(Officer, Nine.AddMinutes(96));

        Assert.Throws<DomainException>(() =>
            visit.CheckIn(Officer, "D1", Signature(), Nine.AddMinutes(200)));
    }

    [Fact]
    public void Duration_is_null_until_both_stamps_exist()
    {
        var visit = NewVisit();
        Assert.Null(visit.Duration);

        visit.CheckIn(Officer, "D1", Signature(), Nine);
        Assert.Null(visit.Duration);
    }
}
