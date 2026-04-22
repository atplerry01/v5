using Whycespace.Domain.CoreSystem.Identifier.GlobalIdentifier;

namespace Whycespace.Tests.Unit.CoreSystem.Identifier;

public sealed class GlobalIdentifierTests
{
    private static readonly string _valid64 = new string('a', 64);
    private static readonly string _valid64b = new string('b', 64);

    // --- Construction ---

    [Fact]
    public void GlobalIdentifier_WithValid64HexLowercase_IsCreated()
    {
        var id = new GlobalIdentifier(_valid64);
        Assert.Equal(_valid64, id.Value);
    }

    [Fact]
    public void GlobalIdentifier_EmptyValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(""));
    }

    [Fact]
    public void GlobalIdentifier_NullValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(null!));
    }

    [Fact]
    public void GlobalIdentifier_63Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(new string('a', 63)));
    }

    [Fact]
    public void GlobalIdentifier_65Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(new string('a', 65)));
    }

    [Fact]
    public void GlobalIdentifier_UppercaseHex_Throws()
    {
        // Must be lowercase only
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(new string('A', 64)));
    }

    [Fact]
    public void GlobalIdentifier_NonHexChar_Throws()
    {
        var invalid = new string('g', 64); // 'g' is not hex
        Assert.ThrowsAny<Exception>(() => new GlobalIdentifier(invalid));
    }

    [Fact]
    public void GlobalIdentifier_ValidHexDigits_IsCreated()
    {
        var allHex = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        var id = new GlobalIdentifier(allHex);
        Assert.Equal(allHex, id.Value);
    }

    // --- IComparable ---

    [Fact]
    public void GlobalIdentifier_CompareTo_IsOrdinal()
    {
        var a = new GlobalIdentifier(new string('a', 64));
        var b = new GlobalIdentifier(new string('b', 64));
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(new GlobalIdentifier(new string('a', 64))));
    }

    [Fact]
    public void GlobalIdentifier_Operators_WorkCorrectly()
    {
        var a = new GlobalIdentifier(_valid64);
        var b = new GlobalIdentifier(_valid64b);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= new GlobalIdentifier(_valid64));
        Assert.True(b >= new GlobalIdentifier(_valid64b));
    }

    [Fact]
    public void GlobalIdentifier_SortedList_IsStable()
    {
        var ids = new[]
        {
            new GlobalIdentifier(new string('c', 64)),
            new GlobalIdentifier(new string('a', 64)),
            new GlobalIdentifier(new string('b', 64))
        };
        var sorted = ids.OrderBy(x => x).ToList();
        Assert.Equal(new string('a', 64), sorted[0].Value);
        Assert.Equal(new string('b', 64), sorted[1].Value);
        Assert.Equal(new string('c', 64), sorted[2].Value);
    }

    // --- Serialization ---

    [Fact]
    public void GlobalIdentifier_ToString_ReturnsValue()
    {
        var id = new GlobalIdentifier(_valid64);
        Assert.Equal(_valid64, id.ToString());
    }

    // --- Equality ---

    [Fact]
    public void GlobalIdentifier_SameValue_AreEqual()
    {
        Assert.Equal(new GlobalIdentifier(_valid64), new GlobalIdentifier(_valid64));
    }

    [Fact]
    public void GlobalIdentifier_DifferentValue_AreNotEqual()
    {
        Assert.NotEqual(new GlobalIdentifier(_valid64), new GlobalIdentifier(_valid64b));
    }
}
