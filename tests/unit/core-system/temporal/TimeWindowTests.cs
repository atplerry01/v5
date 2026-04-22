using Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

namespace Whycespace.Tests.Unit.CoreSystem.Temporal;

public sealed class TimeWindowTests
{
    private static readonly DateTimeOffset _t0 = new(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _t1 = _t0.AddHours(2);
    private static readonly DateTimeOffset _t2 = _t0.AddHours(4);

    // --- Construction ---

    [Fact]
    public void TimeWindow_ClosedWindow_IsCreated()
    {
        var w = new TimeWindow(_t0, _t1);
        Assert.Equal(_t0, w.Start);
        Assert.Equal(_t1, w.End);
        Assert.False(w.IsOpen);
    }

    [Fact]
    public void TimeWindow_OpenWindow_IsCreated()
    {
        var w = new TimeWindow(_t0);
        Assert.Equal(_t0, w.Start);
        Assert.Null(w.End);
        Assert.True(w.IsOpen);
    }

    [Fact]
    public void TimeWindow_NormalizesToUtc()
    {
        var local = new DateTimeOffset(2025, 3, 1, 2, 0, 0, TimeSpan.FromHours(2));
        var w = new TimeWindow(local);
        Assert.Equal(TimeSpan.Zero, w.Start.Offset);
    }

    [Fact]
    public void TimeWindow_DefaultStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeWindow(default));
    }

    [Fact]
    public void TimeWindow_EndBeforeStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeWindow(_t1, _t0));
    }

    [Fact]
    public void TimeWindow_EndEqualToStart_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimeWindow(_t0, _t0));
    }

    // --- Duration ---

    [Fact]
    public void TimeWindow_ClosedWindow_HasDuration()
    {
        var w = new TimeWindow(_t0, _t2);
        Assert.Equal(TimeSpan.FromHours(4), w.Duration);
    }

    [Fact]
    public void TimeWindow_OpenWindow_DurationIsNull()
    {
        var w = new TimeWindow(_t0);
        Assert.Null(w.Duration);
    }

    // --- Contains (half-open [Start, End)) ---

    [Fact]
    public void TimeWindow_Contains_AtStart_ReturnsTrue()
    {
        var w = new TimeWindow(_t0, _t2);
        Assert.True(w.Contains(_t0));
    }

    [Fact]
    public void TimeWindow_Contains_AtEnd_ReturnsFalse()
    {
        var w = new TimeWindow(_t0, _t2);
        Assert.False(w.Contains(_t2));
    }

    [Fact]
    public void TimeWindow_Contains_Interior_ReturnsTrue()
    {
        var w = new TimeWindow(_t0, _t2);
        Assert.True(w.Contains(_t1));
    }

    [Fact]
    public void TimeWindow_OpenWindow_Contains_AlwaysTrueAfterStart()
    {
        var w = new TimeWindow(_t0);
        Assert.True(w.Contains(_t0.AddYears(100)));
        Assert.False(w.Contains(_t0.AddTicks(-1)));
    }

    // --- Overlaps ---

    [Fact]
    public void TimeWindow_TwoClosedOverlapping_ReturnsTrue()
    {
        var a = new TimeWindow(_t0, _t2);
        var b = new TimeWindow(_t1, _t2.AddHours(1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void TimeWindow_OpenOverlapsAnyClosed_ReturnsTrue()
    {
        var open = new TimeWindow(_t0);
        var closed = new TimeWindow(_t1, _t2);
        Assert.True(open.Overlaps(closed));
    }

    [Fact]
    public void TimeWindow_TwoOpenWindows_AlwaysOverlap()
    {
        var a = new TimeWindow(_t0);
        var b = new TimeWindow(_t1);
        Assert.True(a.Overlaps(b));
    }

    // --- ToTimeRange ---

    [Fact]
    public void TimeWindow_ClosedWindow_ConvertToTimeRange_Succeeds()
    {
        var w = new TimeWindow(_t0, _t2);
        var r = w.ToTimeRange();
        Assert.Equal(_t0, r.Start);
        Assert.Equal(_t2, r.End);
    }

    [Fact]
    public void TimeWindow_OpenWindow_ConvertToTimeRange_Throws()
    {
        var w = new TimeWindow(_t0);
        Assert.ThrowsAny<Exception>(() => w.ToTimeRange());
    }

    // --- Serialization ---

    [Fact]
    public void TimeWindow_ClosedWindow_ToString_ContainsBothBounds()
    {
        var w = new TimeWindow(_t0, _t1);
        Assert.Contains(_t0.ToString("O"), w.ToString());
        Assert.Contains(_t1.ToString("O"), w.ToString());
    }

    [Fact]
    public void TimeWindow_OpenWindow_ToString_ContainsInfinity()
    {
        var w = new TimeWindow(_t0);
        Assert.Contains("∞", w.ToString());
    }

    // --- Equality ---

    [Fact]
    public void TimeWindow_SameBounds_AreEqual()
    {
        Assert.Equal(new TimeWindow(_t0, _t1), new TimeWindow(_t0, _t1));
    }

    [Fact]
    public void TimeWindow_OpenVsClosed_AreNotEqual()
    {
        Assert.NotEqual(new TimeWindow(_t0), new TimeWindow(_t0, _t1));
    }
}
