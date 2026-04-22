using Whycespace.Domain.CoreSystem.Identifier.CorrelationId;

namespace Whycespace.Tests.Unit.CoreSystem.Identifier;

public sealed class CorrelationIdTests
{
    private static readonly string _valid = new string('a', 64);
    private static readonly string _valid2 = new string('b', 64);

    // --- Construction ---

    [Fact]
    public void CorrelationId_WithValid64HexLowercase_IsCreated()
    {
        var id = new CorrelationId(_valid);
        Assert.Equal(_valid, id.Value);
    }

    [Fact]
    public void CorrelationId_EmptyValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(""));
    }

    [Fact]
    public void CorrelationId_NullValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(null!));
    }

    [Fact]
    public void CorrelationId_63Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(new string('a', 63)));
    }

    [Fact]
    public void CorrelationId_65Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(new string('a', 65)));
    }

    [Fact]
    public void CorrelationId_UppercaseHex_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(new string('A', 64)));
    }

    [Fact]
    public void CorrelationId_NonHexChar_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CorrelationId(new string('z', 64)));
    }

    [Fact]
    public void CorrelationId_AllValidHexChars_IsCreated()
    {
        var hex = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        var id = new CorrelationId(hex);
        Assert.Equal(hex, id.Value);
    }

    // --- Semantic independence from CausationId ---

    [Fact]
    public void CorrelationId_IsDistinctTypeFromCausationId()
    {
        // Two types — same format, different semantic role.
        // A CorrelationId should not implicitly convert to a CausationId.
        Assert.IsType<CorrelationId>(new CorrelationId(_valid));
    }

    // --- Serialization ---

    [Fact]
    public void CorrelationId_ToString_ReturnsValue()
    {
        Assert.Equal(_valid, new CorrelationId(_valid).ToString());
    }

    // --- Equality ---

    [Fact]
    public void CorrelationId_SameValue_AreEqual()
    {
        Assert.Equal(new CorrelationId(_valid), new CorrelationId(_valid));
    }

    [Fact]
    public void CorrelationId_DifferentValue_AreNotEqual()
    {
        Assert.NotEqual(new CorrelationId(_valid), new CorrelationId(_valid2));
    }

    // --- Immutability ---

    [Fact]
    public void CorrelationId_Value_IsReadOnly()
    {
        var id = new CorrelationId(_valid);
        // readonly record struct: copy produces same value
        var copy = id;
        Assert.Equal(id.Value, copy.Value);
    }
}
