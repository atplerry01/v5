using Whycespace.Domain.CoreSystem.Ordering.OrderingKey;

namespace Whycespace.Tests.Unit.CoreSystem.Ordering;

public sealed class OrderingKeyTests
{
    // --- Construction ---

    [Fact]
    public void OrderingKey_WithValue_IsCreated()
    {
        var k = new OrderingKey("created-at");
        Assert.Equal("created-at", k.Value);
    }

    [Fact]
    public void OrderingKey_EmptyValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new OrderingKey(""));
    }

    [Fact]
    public void OrderingKey_NullValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new OrderingKey(null!));
    }

    // --- Ordinal comparison (culture-invariant) ---

    [Fact]
    public void OrderingKey_CompareTo_IsOrdinal()
    {
        var a = new OrderingKey("a");
        var b = new OrderingKey("b");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(new OrderingKey("a")));
    }

    [Fact]
    public void OrderingKey_CaseIsSignificant()
    {
        // Ordinal: uppercase < lowercase
        var upper = new OrderingKey("A");
        var lower = new OrderingKey("a");
        Assert.True(upper.CompareTo(lower) < 0);
    }

    [Fact]
    public void OrderingKey_Precedes_ReturnsTrue_WhenLexicallyBefore()
    {
        Assert.True(new OrderingKey("alpha").Precedes(new OrderingKey("beta")));
    }

    [Fact]
    public void OrderingKey_Follows_ReturnsTrue_WhenLexicallyAfter()
    {
        Assert.True(new OrderingKey("z").Follows(new OrderingKey("a")));
    }

    [Fact]
    public void OrderingKey_Operators_WorkCorrectly()
    {
        var a = new OrderingKey("aaa");
        var b = new OrderingKey("bbb");
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= new OrderingKey("aaa"));
        Assert.True(b >= new OrderingKey("bbb"));
    }

    // --- Determinism ---

    [Fact]
    public void OrderingKey_SameValue_SameComparisonResult_AcrossInstances()
    {
        var x = new OrderingKey("key-x");
        var y = new OrderingKey("key-y");
        Assert.Equal(
            new OrderingKey("key-x").CompareTo(new OrderingKey("key-y")),
            x.CompareTo(y));
    }

    // --- Serialization ---

    [Fact]
    public void OrderingKey_ToString_ReturnsValue()
    {
        Assert.Equal("my-key", new OrderingKey("my-key").ToString());
    }

    // --- Equality ---

    [Fact]
    public void OrderingKey_SameValue_AreEqual()
    {
        Assert.Equal(new OrderingKey("x"), new OrderingKey("x"));
    }

    [Fact]
    public void OrderingKey_DifferentValue_AreNotEqual()
    {
        Assert.NotEqual(new OrderingKey("x"), new OrderingKey("y"));
    }
}
