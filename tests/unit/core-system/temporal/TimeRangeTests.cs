using Whycespace.Domain.CoreSystem.Temporal.TimeRange;

namespace Whycespace.Tests.Unit.CoreSystem.Temporal;

public sealed class TimeRangeTests
{
    private static readonly DateTimeOffset _t0 = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _t1 = _t0.AddHours(1);
    private static readonly DateTimeOffset _t2 = _t0.AddHours(2);
    private static readonly DateTimeOffset _t3 = _t0.AddHours(3);

    // --- Construction ---

    [Fact]
    public void TimeRange_WithValidBounds_IsCreated()
    {
        var r = new TimeRange(_t0, _t1);
        Assert.Equal(_t0, r.Start);
        Assert.Equal(_t1, r.End);
    }

    [Fact]
    public void TimeRange_NormalizesToUtc()
    {
        var localT0 = new DateTimeOffset(2025, 1, 1, 2, 0, 0, TimeSpan.FromHours(2));
        var localT1 = new DateTimeOffset(2025, 1, 1, 4, 0, 0, TimeSpan.FromHours(2));
        var r = new TimeRange(localT0, localT1);
        Assert.Equal(TimeSpan.Zero, r.Start.Offset);
        Assert.Equal(TimeSpan.Zero, r.End.Offset);
    }

    [Fact]
    public void TimeRange_EndEqualToStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeRange(_t0, _t0));
    }

    [Fact]
    public void TimeRange_EndBeforeStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeRange(_t1, _t0));
    }

    [Fact]
    public void TimeRange_DefaultStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeRange(default, _t1));
    }

    [Fact]
    public void TimeRange_DefaultEnd_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeRange(_t0, default));
    }

    // --- Duration / Midpoint ---

    [Fact]
    public void TimeRange_Duration_IsEndMinusStart()
    {
        var r = new TimeRange(_t0, _t2);
        Assert.Equal(TimeSpan.FromHours(2), r.Duration);
    }

    [Fact]
    public void TimeRange_Midpoint_IsHalfwayPoint()
    {
        var r = new TimeRange(_t0, _t2);
        Assert.Equal(_t1, r.Midpoint);
    }

    // --- Contains (closed interval [Start, End]) ---

    [Fact]
    public void TimeRange_Contains_StartBoundary_ReturnsTrue()
    {
        var r = new TimeRange(_t0, _t2);
        Assert.True(r.Contains(_t0));
    }

    [Fact]
    public void TimeRange_Contains_EndBoundary_ReturnsTrue()
    {
        var r = new TimeRange(_t0, _t2);
        Assert.True(r.Contains(_t2));
    }

    [Fact]
    public void TimeRange_Contains_Interior_ReturnsTrue()
    {
        var r = new TimeRange(_t0, _t2);
        Assert.True(r.Contains(_t1));
    }

    [Fact]
    public void TimeRange_Contains_Outside_ReturnsFalse()
    {
        var r = new TimeRange(_t0, _t1);
        Assert.False(r.Contains(_t2));
    }

    // --- Overlaps ---

    [Fact]
    public void TimeRange_Overlapping_ReturnsTrue()
    {
        var a = new TimeRange(_t0, _t2);
        var b = new TimeRange(_t1, _t3);
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void TimeRange_Adjacent_DoesNotOverlap()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t1, _t2);
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void TimeRange_Disjoint_DoesNotOverlap()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t2, _t3);
        Assert.False(a.Overlaps(b));
    }

    // --- Precedes / Follows ---

    [Fact]
    public void TimeRange_Precedes_ReturnsTrue_WhenBefore()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t2, _t3);
        Assert.True(a.Precedes(b));
        Assert.False(b.Precedes(a));
    }

    [Fact]
    public void TimeRange_Follows_ReturnsTrue_WhenAfter()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t2, _t3);
        Assert.True(b.Follows(a));
        Assert.False(a.Follows(b));
    }

    // --- IsAdjacent ---

    [Fact]
    public void TimeRange_IsAdjacent_ReturnsTrue_WhenTouching()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t1, _t2);
        Assert.True(a.IsAdjacent(b));
        Assert.True(b.IsAdjacent(a));
    }

    [Fact]
    public void TimeRange_IsAdjacent_ReturnsFalse_WhenGap()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t2, _t3);
        Assert.False(a.IsAdjacent(b));
    }

    // --- Gap ---

    [Fact]
    public void TimeRange_Gap_ReturnsNull_WhenOverlapping()
    {
        var a = new TimeRange(_t0, _t2);
        var b = new TimeRange(_t1, _t3);
        Assert.Null(a.Gap(b));
    }

    [Fact]
    public void TimeRange_Gap_ReturnsNull_WhenAdjacent()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t1, _t2);
        Assert.Null(a.Gap(b));
    }

    [Fact]
    public void TimeRange_Gap_ReturnsGapDuration_WhenDisjoint()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t3, _t3.AddHours(1));
        var gap = a.Gap(b);
        Assert.NotNull(gap);
        Assert.Equal(_t3 - _t1, gap!.Value);
    }

    [Fact]
    public void TimeRange_Gap_IsSymmetric()
    {
        var a = new TimeRange(_t0, _t1);
        var b = new TimeRange(_t3, _t3.AddHours(1));
        Assert.Equal(a.Gap(b), b.Gap(a));
    }

    // --- Serialization ---

    [Fact]
    public void TimeRange_ToString_ContainsBothBoundaries()
    {
        var r = new TimeRange(_t0, _t1);
        var s = r.ToString();
        Assert.Contains(_t0.ToString("O"), s);
        Assert.Contains(_t1.ToString("O"), s);
    }

    // --- Equality ---

    [Fact]
    public void TimeRange_SameBounds_AreEqual()
    {
        Assert.Equal(new TimeRange(_t0, _t1), new TimeRange(_t0, _t1));
    }

    [Fact]
    public void TimeRange_DifferentBounds_AreNotEqual()
    {
        Assert.NotEqual(new TimeRange(_t0, _t1), new TimeRange(_t0, _t2));
    }
}
