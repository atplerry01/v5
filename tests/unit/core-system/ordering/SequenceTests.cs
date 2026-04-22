using Whycespace.Domain.CoreSystem.Ordering.Sequence;

namespace Whycespace.Tests.Unit.CoreSystem.Ordering;

public sealed class SequenceTests
{
    // --- Construction ---

    [Fact]
    public void Sequence_Zero_HasValueZero()
    {
        Assert.Equal(0L, Sequence.Zero.Value);
    }

    [Fact]
    public void Sequence_WithPositiveValue_IsCreated()
    {
        var s = new Sequence(42);
        Assert.Equal(42L, s.Value);
    }

    [Fact]
    public void Sequence_WithNegativeValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new Sequence(-1));
    }

    // --- Monotonicity ---

    [Fact]
    public void Sequence_Next_IsValuePlusOne()
    {
        var s = new Sequence(5);
        Assert.Equal(6L, s.Next().Value);
    }

    [Fact]
    public void Sequence_Next_FromZero_IsOne()
    {
        Assert.Equal(1L, Sequence.Zero.Next().Value);
    }

    [Fact]
    public void Sequence_ChainedNext_IsMonotonic()
    {
        var s = Sequence.Zero;
        for (var i = 1; i <= 10; i++)
        {
            s = s.Next();
            Assert.Equal((long)i, s.Value);
        }
    }

    // --- Comparison ---

    [Fact]
    public void Sequence_Precedes_ReturnsTrueForSmaller()
    {
        var a = new Sequence(1);
        var b = new Sequence(2);
        Assert.True(a.Precedes(b));
        Assert.False(b.Precedes(a));
    }

    [Fact]
    public void Sequence_Follows_ReturnsTrueForLarger()
    {
        var a = new Sequence(1);
        var b = new Sequence(2);
        Assert.True(b.Follows(a));
        Assert.False(a.Follows(b));
    }

    [Fact]
    public void Sequence_CompareTo_Equal_ReturnsZero()
    {
        Assert.Equal(0, new Sequence(5).CompareTo(new Sequence(5)));
    }

    [Fact]
    public void Sequence_Operators_WorkCorrectly()
    {
        var a = new Sequence(1);
        var b = new Sequence(2);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= new Sequence(1));
        Assert.True(b >= new Sequence(2));
    }

    // --- SequenceRange ---

    [Fact]
    public void SequenceRange_WithValidBounds_IsCreated()
    {
        var r = new SequenceRange(new Sequence(1), new Sequence(5));
        Assert.Equal(1L, r.Start.Value);
        Assert.Equal(5L, r.End.Value);
    }

    [Fact]
    public void SequenceRange_EndBeforeStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new SequenceRange(new Sequence(5), new Sequence(1)));
    }

    [Fact]
    public void SequenceRange_EqualBounds_IsValid()
    {
        var r = new SequenceRange(new Sequence(3), new Sequence(3));
        Assert.Equal(1L, r.Count);
    }

    [Fact]
    public void SequenceRange_Count_IsCorrect()
    {
        var r = new SequenceRange(new Sequence(1), new Sequence(5));
        Assert.Equal(5L, r.Count);
    }

    [Fact]
    public void SequenceRange_Contains_Start_ReturnsTrue()
    {
        var r = new SequenceRange(new Sequence(2), new Sequence(8));
        Assert.True(r.Contains(new Sequence(2)));
    }

    [Fact]
    public void SequenceRange_Contains_End_ReturnsTrue()
    {
        var r = new SequenceRange(new Sequence(2), new Sequence(8));
        Assert.True(r.Contains(new Sequence(8)));
    }

    [Fact]
    public void SequenceRange_Contains_Outside_ReturnsFalse()
    {
        var r = new SequenceRange(new Sequence(2), new Sequence(4));
        Assert.False(r.Contains(new Sequence(5)));
    }

    // --- Serialization ---

    [Fact]
    public void Sequence_ToString_IsValueString()
    {
        Assert.Equal("7", new Sequence(7).ToString());
    }

    // --- Equality ---

    [Fact]
    public void Sequence_SameValue_AreEqual()
    {
        Assert.Equal(new Sequence(10), new Sequence(10));
    }

    [Fact]
    public void Sequence_DifferentValue_AreNotEqual()
    {
        Assert.NotEqual(new Sequence(10), new Sequence(11));
    }
}
