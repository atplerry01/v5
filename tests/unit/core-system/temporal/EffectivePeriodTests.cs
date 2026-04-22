using Whycespace.Domain.CoreSystem.Temporal.EffectivePeriod;

namespace Whycespace.Tests.Unit.CoreSystem.Temporal;

public sealed class EffectivePeriodTests
{
    private static readonly DateTimeOffset _past = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _mid = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _future = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // --- Construction ---

    [Fact]
    public void EffectivePeriod_Unbounded_IsAlways()
    {
        var p = EffectivePeriod.Always;
        Assert.Null(p.EffectiveFrom);
        Assert.Null(p.EffectiveTo);
    }

    [Fact]
    public void EffectivePeriod_WithBothBounds_IsCreated()
    {
        var p = new EffectivePeriod(_past, _future);
        Assert.Equal(_past, p.EffectiveFrom);
        Assert.Equal(_future, p.EffectiveTo);
    }

    [Fact]
    public void EffectivePeriod_NormalizesToUtc()
    {
        var local = new DateTimeOffset(2025, 1, 1, 2, 0, 0, TimeSpan.FromHours(2));
        var p = new EffectivePeriod(local);
        Assert.Equal(TimeSpan.Zero, p.EffectiveFrom!.Value.Offset);
    }

    [Fact]
    public void EffectivePeriod_ToBeforeFrom_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EffectivePeriod(_future, _past));
    }

    [Fact]
    public void EffectivePeriod_ToEqualFrom_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new EffectivePeriod(_past, _past));
    }

    // --- IsActive ---

    [Fact]
    public void EffectivePeriod_Always_IsActiveAtAnyPoint()
    {
        var p = EffectivePeriod.Always;
        Assert.True(p.IsActive(DateTimeOffset.MinValue));
        Assert.True(p.IsActive(DateTimeOffset.MaxValue));
    }

    [Fact]
    public void EffectivePeriod_Bounded_IsActive_WithinBounds()
    {
        var p = new EffectivePeriod(_past, _future);
        Assert.True(p.IsActive(_mid));
    }

    [Fact]
    public void EffectivePeriod_Bounded_IsActive_AtFrom_ReturnsTrue()
    {
        var p = new EffectivePeriod(_past, _future);
        Assert.True(p.IsActive(_past));
    }

    [Fact]
    public void EffectivePeriod_Bounded_IsActive_AtTo_ReturnsFalse()
    {
        var p = new EffectivePeriod(_past, _future);
        Assert.False(p.IsActive(_future));
    }

    [Fact]
    public void EffectivePeriod_Bounded_IsNotActive_Before()
    {
        var p = new EffectivePeriod(_mid, _future);
        Assert.False(p.IsActive(_past));
    }

    // --- HasStarted / HasExpired ---

    [Fact]
    public void EffectivePeriod_HasStarted_WithNoFrom_ReturnsTrue()
    {
        var p = new EffectivePeriod(null, _future);
        Assert.True(p.HasStarted(_past));
    }

    [Fact]
    public void EffectivePeriod_HasStarted_BeforeFrom_ReturnsFalse()
    {
        var p = new EffectivePeriod(_mid, _future);
        Assert.False(p.HasStarted(_past));
    }

    [Fact]
    public void EffectivePeriod_HasExpired_WithNoTo_ReturnsFalse()
    {
        var p = new EffectivePeriod(_past, null);
        Assert.False(p.HasExpired(_future));
    }

    [Fact]
    public void EffectivePeriod_HasExpired_AfterTo_ReturnsTrue()
    {
        var p = new EffectivePeriod(_past, _mid);
        Assert.True(p.HasExpired(_future));
    }

    // --- Overlaps ---

    [Fact]
    public void EffectivePeriod_TwoBounded_Overlapping_ReturnsTrue()
    {
        var a = new EffectivePeriod(_past, _future);
        var b = new EffectivePeriod(_mid, _future.AddYears(5));
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void EffectivePeriod_BoundedAndAlways_Overlap()
    {
        var bounded = new EffectivePeriod(_past, _future);
        Assert.True(bounded.Overlaps(EffectivePeriod.Always));
    }

    [Fact]
    public void EffectivePeriod_TwoNonOverlapping_ReturnsFalse()
    {
        var a = new EffectivePeriod(_past, _mid);
        var b = new EffectivePeriod(_future, _future.AddYears(1));
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    // --- Serialization ---

    [Fact]
    public void EffectivePeriod_Always_ToString_ContainsInfinity()
    {
        var s = EffectivePeriod.Always.ToString();
        Assert.Contains("−∞", s);
        Assert.Contains("+∞", s);
    }

    [Fact]
    public void EffectivePeriod_Bounded_ToString_ContainsBothBounds()
    {
        var p = new EffectivePeriod(_past, _future);
        Assert.Contains(_past.ToString("O"), p.ToString());
        Assert.Contains(_future.ToString("O"), p.ToString());
    }

    // --- Equality ---

    [Fact]
    public void EffectivePeriod_SameBounds_AreEqual()
    {
        Assert.Equal(new EffectivePeriod(_past, _future), new EffectivePeriod(_past, _future));
    }

    [Fact]
    public void EffectivePeriod_DifferentBounds_AreNotEqual()
    {
        Assert.NotEqual(new EffectivePeriod(_past, _future), new EffectivePeriod(_mid, _future));
    }
}
