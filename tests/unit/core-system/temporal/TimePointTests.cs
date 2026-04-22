using Whycespace.Domain.CoreSystem.Temporal.TimePoint;

namespace Whycespace.Tests.Unit.CoreSystem.Temporal;

public sealed class TimePointTests
{
    private static readonly DateTimeOffset _utc = new(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _plus2 = new(2025, 6, 15, 14, 0, 0, TimeSpan.FromHours(2));

    // --- Construction ---

    [Fact]
    public void TimePoint_WithValidUtc_StoresTimestamp()
    {
        var tp = new TimePoint(_utc);
        Assert.Equal(_utc, tp.Timestamp);
    }

    [Fact]
    public void TimePoint_WithNonUtcOffset_NormalizesToUtc()
    {
        var tp = new TimePoint(_plus2);
        Assert.Equal(_plus2.ToUniversalTime(), tp.Timestamp);
        Assert.Equal(TimeSpan.Zero, tp.Timestamp.Offset);
    }

    [Fact]
    public void TimePoint_WithDefault_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new TimePoint(default));
    }

    [Fact]
    public void TimePoint_WithLabel_StoresLabel()
    {
        var tp = new TimePoint(_utc, "start");
        Assert.Equal("start", tp.Label);
    }

    [Fact]
    public void TimePoint_WithNoLabel_LabelIsNull()
    {
        var tp = new TimePoint(_utc);
        Assert.Null(tp.Label);
    }

    // --- Comparison ---

    [Fact]
    public void TimePoint_IsBefore_ReturnsTrue_WhenEarlier()
    {
        var earlier = new TimePoint(_utc);
        var later = new TimePoint(_utc.AddSeconds(1));
        Assert.True(earlier.IsBefore(later));
        Assert.False(later.IsBefore(earlier));
    }

    [Fact]
    public void TimePoint_IsAfter_ReturnsTrue_WhenLater()
    {
        var earlier = new TimePoint(_utc);
        var later = new TimePoint(_utc.AddSeconds(1));
        Assert.True(later.IsAfter(earlier));
        Assert.False(earlier.IsAfter(later));
    }

    [Fact]
    public void TimePoint_IsSameInstant_ReturnsTrue_WhenEqual()
    {
        var a = new TimePoint(_utc);
        var b = new TimePoint(_utc);
        Assert.True(a.IsSameInstant(b));
    }

    [Fact]
    public void TimePoint_NonUtcAndUtcSameInstant_AreEqual()
    {
        var a = new TimePoint(_plus2);
        var b = new TimePoint(_plus2.ToUniversalTime());
        Assert.True(a.IsSameInstant(b));
    }

    [Fact]
    public void TimePoint_CompareTo_OrdersChronologically()
    {
        var earlier = new TimePoint(_utc);
        var later = new TimePoint(_utc.AddMinutes(5));
        Assert.True(earlier.CompareTo(later) < 0);
        Assert.True(later.CompareTo(earlier) > 0);
        Assert.Equal(0, earlier.CompareTo(new TimePoint(_utc)));
    }

    [Fact]
    public void TimePoint_Operators_WorkCorrectly()
    {
        var a = new TimePoint(_utc);
        var b = new TimePoint(_utc.AddSeconds(1));
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= new TimePoint(_utc));
        Assert.True(a >= new TimePoint(_utc));
    }

    // --- DistanceTo ---

    [Fact]
    public void TimePoint_DistanceTo_IsAbsolute()
    {
        var a = new TimePoint(_utc);
        var b = new TimePoint(_utc.AddHours(3));
        Assert.Equal(TimeSpan.FromHours(3), a.DistanceTo(b));
        Assert.Equal(TimeSpan.FromHours(3), b.DistanceTo(a));
    }

    // --- Serialization ---

    [Fact]
    public void TimePoint_ToString_ProducesIso8601()
    {
        var tp = new TimePoint(_utc);
        Assert.Contains("+00:00", tp.ToString());
    }

    [Fact]
    public void TimePoint_ToString_WithLabel_IncludesLabel()
    {
        var tp = new TimePoint(_utc, "deadline");
        Assert.StartsWith("deadline=", tp.ToString());
    }

    // --- Immutability (structural) ---

    [Fact]
    public void TimePoint_IsImmutable_WhenCreated()
    {
        var tp = new TimePoint(_utc);
        var copy = tp with { };
        Assert.Equal(tp.Timestamp, copy.Timestamp);
    }

    // --- Equality ---

    [Fact]
    public void TimePoint_SameTimestamp_AreEqual()
    {
        var a = new TimePoint(_utc);
        var b = new TimePoint(_utc);
        Assert.Equal(a, b);
    }

    [Fact]
    public void TimePoint_DifferentTimestamp_AreNotEqual()
    {
        var a = new TimePoint(_utc);
        var b = new TimePoint(_utc.AddTicks(1));
        Assert.NotEqual(a, b);
    }
}
