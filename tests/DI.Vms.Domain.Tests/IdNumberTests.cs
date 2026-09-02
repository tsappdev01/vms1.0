using DI.Vms.Domain.Common;
using DI.Vms.Domain.Enums;
using DI.Vms.Domain.ValueObjects;
using Xunit;

namespace DI.Vms.Domain.Tests;

public class IdNumberTests
{
    [Theory]
    [InlineData("784-1985-1234567-1")]
    [InlineData("784 1985 1234567 1")]
    [InlineData("784198512345671")]
    public void Normalise_strips_separators_from_an_emirates_id(string input)
    {
        Assert.Equal("784198512345671", IdNumber.Normalise(input, IdType.EmiratesId));
    }

    /// <summary>
    /// The same person scanned twice with different formatting must produce one visitor,
    /// or repeat-visitor recognition misses and the uniqueness index admits duplicates.
    /// </summary>
    [Fact]
    public void Equal_ids_written_differently_compare_equal()
    {
        var a = IdNumber.Create("784-1985-1234567-1", IdType.EmiratesId);
        var b = IdNumber.Create("784198512345671", IdType.EmiratesId);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Emirates_id_masks_to_the_form_the_brd_requires()
    {
        var id = IdNumber.Create("784198512345671", IdType.EmiratesId);
        Assert.Equal("784-XXXX-XXXXXXX-1", id.Masked);
    }

    /// <summary>
    /// ToString returns the masked form deliberately, so an accidental interpolation in a
    /// log line or an exception message cannot leak the number.
    /// </summary>
    [Fact]
    public void ToString_never_returns_the_plaintext()
    {
        var id = IdNumber.Create("784198512345671", IdType.EmiratesId);

        Assert.DoesNotContain("784198512345671", id.ToString());
        Assert.Equal(id.Masked, id.ToString());
    }

    [Fact]
    public void Other_id_types_keep_only_the_last_four_characters()
    {
        var id = IdNumber.Create("P8821445", IdType.Passport);
        Assert.Equal("XXXX1445", id.Masked);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("784198512345")]
    [InlineData("not-a-number")]
    public void Invalid_emirates_ids_are_rejected(string input)
    {
        Assert.Throws<DomainException>(() => IdNumber.Create(input, IdType.EmiratesId));
    }

    [Fact]
    public void Masking_a_short_value_reveals_nothing()
    {
        Assert.Equal("XXXX", IdNumber.Mask("1234", IdType.Passport));
    }

    [Fact]
    public void Masking_an_unparseable_emirates_id_reveals_nothing()
    {
        Assert.Equal("XXXXXXXXXXXXXXX", IdNumber.Mask("12345", IdType.EmiratesId));
    }
}
