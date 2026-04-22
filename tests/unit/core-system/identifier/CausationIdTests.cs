using Whycespace.Domain.CoreSystem.Identifier.CausationId;

namespace Whycespace.Tests.Unit.CoreSystem.Identifier;

public sealed class CausationIdTests
{
    private static readonly string _valid = new string('c', 64);
    private static readonly string _valid2 = new string('d', 64);

    // --- Construction ---

    [Fact]
    public void CausationId_WithValid64HexLowercase_IsCreated()
    {
        var id = new CausationId(_valid);
        Assert.Equal(_valid, id.Value);
    }

    [Fact]
    public void CausationId_EmptyValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(""));
    }

    [Fact]
    public void CausationId_NullValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(null!));
    }

    [Fact]
    public void CausationId_63Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(new string('a', 63)));
    }

    [Fact]
    public void CausationId_65Chars_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(new string('a', 65)));
    }

    [Fact]
    public void CausationId_UppercaseHex_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(new string('C', 64)));
    }

    [Fact]
    public void CausationId_NonHexChar_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CausationId(new string('x', 64)));
    }

    [Fact]
    public void CausationId_AllValidHexChars_IsCreated()
    {
        var hex = "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210";
        var id = new CausationId(hex);
        Assert.Equal(hex, id.Value);
    }

    // --- Causation chain semantics ---

    [Fact]
    public void CausationId_DifferentValues_RepresentDifferentCauses()
    {
        var cause1 = new CausationId(_valid);
        var cause2 = new CausationId(_valid2);
        Assert.NotEqual(cause1, cause2);
    }

    [Fact]
    public void CausationId_SameValue_RepresentSameCause()
    {
        var cause1 = new CausationId(_valid);
        var cause2 = new CausationId(_valid);
        Assert.Equal(cause1, cause2);
    }

    // --- Serialization ---

    [Fact]
    public void CausationId_ToString_ReturnsValue()
    {
        Assert.Equal(_valid, new CausationId(_valid).ToString());
    }

    // --- Immutability ---

    [Fact]
    public void CausationId_Value_IsReadOnly()
    {
        var id = new CausationId(_valid);
        var copy = id;
        Assert.Equal(id.Value, copy.Value);
    }

    // --- Equality ---

    [Fact]
    public void CausationId_SameValue_AreEqual()
    {
        Assert.Equal(new CausationId(_valid), new CausationId(_valid));
    }

    [Fact]
    public void CausationId_DifferentValue_AreNotEqual()
    {
        Assert.NotEqual(new CausationId(_valid), new CausationId(_valid2));
    }
}
