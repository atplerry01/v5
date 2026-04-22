using Whycespace.Domain.CoreSystem.Identifier.EntityReference;

namespace Whycespace.Tests.Unit.CoreSystem.Identifier;

public sealed class EntityReferenceTests
{
    private static readonly string _validId = new string('a', 64);
    private static readonly string _validId2 = new string('b', 64);
    private const string _validType = "economic-system/capital/vault";

    // --- Construction ---

    [Fact]
    public void EntityReference_WithValidInputs_IsCreated()
    {
        var ref1 = new EntityReference(_validId, _validType);
        Assert.Equal(_validId, ref1.IdentifierValue);
        Assert.Equal(_validType, ref1.EntityType);
    }

    [Fact]
    public void EntityReference_EmptyIdentifier_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference("", _validType));
    }

    [Fact]
    public void EntityReference_Non64HexIdentifier_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference("short", _validType));
    }

    [Fact]
    public void EntityReference_EmptyEntityType_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference(_validId, ""));
    }

    [Fact]
    public void EntityReference_TwoSegmentType_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference(_validId, "economic-system/capital"));
    }

    [Fact]
    public void EntityReference_FourSegmentType_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference(_validId, "a/b/c/d"));
    }

    [Fact]
    public void EntityReference_TypeWithEmptySegment_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EntityReference(_validId, "a//c"));
    }

    // --- IsSameEntity ---

    [Fact]
    public void EntityReference_IsSameEntity_SameTypeAndId_ReturnsTrue()
    {
        var a = new EntityReference(_validId, _validType);
        var b = new EntityReference(_validId, _validType);
        Assert.True(a.IsSameEntity(b));
    }

    [Fact]
    public void EntityReference_IsSameEntity_DifferentId_ReturnsFalse()
    {
        var a = new EntityReference(_validId, _validType);
        var b = new EntityReference(_validId2, _validType);
        Assert.False(a.IsSameEntity(b));
    }

    [Fact]
    public void EntityReference_IsSameEntity_DifferentType_ReturnsFalse()
    {
        var a = new EntityReference(_validId, _validType);
        var b = new EntityReference(_validId, "trust-system/identity/credential");
        Assert.False(a.IsSameEntity(b));
    }

    [Fact]
    public void EntityReference_IsSameEntity_IsSymmetric()
    {
        var a = new EntityReference(_validId, _validType);
        var b = new EntityReference(_validId, _validType);
        Assert.Equal(a.IsSameEntity(b), b.IsSameEntity(a));
    }

    // --- Serialization ---

    [Fact]
    public void EntityReference_ToString_CombinesTypeAndId()
    {
        var ref1 = new EntityReference(_validId, _validType);
        Assert.Equal($"{_validType}:{_validId}", ref1.ToString());
    }

    // --- Equality ---

    [Fact]
    public void EntityReference_SameInputs_AreEqual()
    {
        Assert.Equal(
            new EntityReference(_validId, _validType),
            new EntityReference(_validId, _validType));
    }

    [Fact]
    public void EntityReference_DifferentId_AreNotEqual()
    {
        Assert.NotEqual(
            new EntityReference(_validId, _validType),
            new EntityReference(_validId2, _validType));
    }
}
