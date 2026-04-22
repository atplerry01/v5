using System.Text.Json;
using Whycespace.Domain.CoreSystem.Temporal.EffectivePeriod;
using Whycespace.Domain.CoreSystem.Temporal.TimePoint;
using Whycespace.Domain.CoreSystem.Temporal.TimeRange;
using Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

namespace Whycespace.Tests.Unit.CoreSystem.Temporal;

/// <summary>
/// E12 — Serialization roundtrip, timezone drift, and precision tests for
/// all four temporal primitives. Covers topics 5 (boundary rules), 11 (runtime
/// integration), 13 (persistence), 14 (messaging), 20 (resilience) of core-system.md.
/// </summary>
public sealed class CoreSystemTemporalSerializationTests
{
    private static readonly DateTimeOffset _utc = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _utcEnd = new(2025, 6, 1, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _nonUtcPlus2 = new(2025, 6, 1, 14, 0, 0, TimeSpan.FromHours(2));

    // -------------------------------------------------------------------------
    // TimePoint — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void TimePoint_Serializes_WithUtcTimestampProperty()
    {
        var tp = new TimePoint(_utc);
        var json = JsonSerializer.Serialize(tp);
        using var doc = JsonDocument.Parse(json);
        var stored = doc.RootElement.GetProperty("Timestamp").GetDateTimeOffset();
        Assert.Equal(TimeSpan.Zero, stored.Offset);
        Assert.Equal(_utc, stored);
    }

    [Fact]
    public void TimePoint_Roundtrip_WithoutLabel_PreservesInstant()
    {
        var original = new TimePoint(_utc);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var timestamp = doc.RootElement.GetProperty("Timestamp").GetDateTimeOffset();
        var reconstructed = new TimePoint(timestamp);
        Assert.True(original.IsSameInstant(reconstructed));
    }

    [Fact]
    public void TimePoint_Roundtrip_WithLabel_PreservesLabelAndInstant()
    {
        var original = new TimePoint(_utc, "settlement-deadline");
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var timestamp = doc.RootElement.GetProperty("Timestamp").GetDateTimeOffset();
        var label = doc.RootElement.GetProperty("Label").GetString();
        var reconstructed = new TimePoint(timestamp, label);
        Assert.True(original.IsSameInstant(reconstructed));
        Assert.Equal("settlement-deadline", reconstructed.Label);
    }

    [Fact]
    public void TimePoint_Serialization_IsStable_SameInputSameJson()
    {
        var tp = new TimePoint(_utc, "marker");
        Assert.Equal(JsonSerializer.Serialize(tp), JsonSerializer.Serialize(tp));
    }

    // -------------------------------------------------------------------------
    // TimeRange — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void TimeRange_Serializes_WithStartAndEndProperties()
    {
        var r = new TimeRange(_utc, _utcEnd);
        var json = JsonSerializer.Serialize(r);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(TimeSpan.Zero, doc.RootElement.GetProperty("Start").GetDateTimeOffset().Offset);
        Assert.Equal(TimeSpan.Zero, doc.RootElement.GetProperty("End").GetDateTimeOffset().Offset);
    }

    [Fact]
    public void TimeRange_Roundtrip_PreservesBothBoundaries()
    {
        var original = new TimeRange(_utc, _utcEnd);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var start = doc.RootElement.GetProperty("Start").GetDateTimeOffset();
        var end = doc.RootElement.GetProperty("End").GetDateTimeOffset();
        var reconstructed = new TimeRange(start, end);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void TimeRange_Serialization_IsStable()
    {
        var r = new TimeRange(_utc, _utcEnd);
        Assert.Equal(JsonSerializer.Serialize(r), JsonSerializer.Serialize(r));
    }

    // -------------------------------------------------------------------------
    // TimeWindow — JSON roundtrip (open and closed)
    // -------------------------------------------------------------------------

    [Fact]
    public void TimeWindow_Closed_Roundtrip_PreservesStartAndEnd()
    {
        var original = new TimeWindow(_utc, _utcEnd);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var start = doc.RootElement.GetProperty("Start").GetDateTimeOffset();
        var endEl = doc.RootElement.GetProperty("End");
        DateTimeOffset? end = endEl.ValueKind == JsonValueKind.Null
            ? null
            : endEl.GetDateTimeOffset();
        var reconstructed = new TimeWindow(start, end);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void TimeWindow_Open_Roundtrip_EndIsNull()
    {
        var original = new TimeWindow(_utc);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("End").ValueKind);
        var start = doc.RootElement.GetProperty("Start").GetDateTimeOffset();
        var reconstructed = new TimeWindow(start);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void TimeWindow_Serialization_IsStable()
    {
        var w = new TimeWindow(_utc, _utcEnd);
        Assert.Equal(JsonSerializer.Serialize(w), JsonSerializer.Serialize(w));
    }

    // -------------------------------------------------------------------------
    // EffectivePeriod — JSON roundtrip (unbounded and bounded)
    // -------------------------------------------------------------------------

    [Fact]
    public void EffectivePeriod_Always_Serializes_WithBothNullBounds()
    {
        var json = JsonSerializer.Serialize(EffectivePeriod.Always);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("EffectiveFrom").ValueKind);
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("EffectiveTo").ValueKind);
    }

    [Fact]
    public void EffectivePeriod_Always_Roundtrip_RemainsAlways()
    {
        var original = EffectivePeriod.Always;
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var fromEl = doc.RootElement.GetProperty("EffectiveFrom");
        var toEl = doc.RootElement.GetProperty("EffectiveTo");
        DateTimeOffset? from = fromEl.ValueKind == JsonValueKind.Null ? null : fromEl.GetDateTimeOffset();
        DateTimeOffset? to = toEl.ValueKind == JsonValueKind.Null ? null : toEl.GetDateTimeOffset();
        var reconstructed = new EffectivePeriod(from, to);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void EffectivePeriod_Bounded_Roundtrip_PreservesBothBounds()
    {
        var original = new EffectivePeriod(_utc, _utcEnd);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var from = doc.RootElement.GetProperty("EffectiveFrom").GetDateTimeOffset();
        var to = doc.RootElement.GetProperty("EffectiveTo").GetDateTimeOffset();
        var reconstructed = new EffectivePeriod(from, to);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void EffectivePeriod_HalfBounded_FromOnly_Roundtrip()
    {
        var original = new EffectivePeriod(_utc, null);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var fromEl = doc.RootElement.GetProperty("EffectiveFrom");
        var toEl = doc.RootElement.GetProperty("EffectiveTo");
        DateTimeOffset? from = fromEl.ValueKind == JsonValueKind.Null ? null : fromEl.GetDateTimeOffset();
        DateTimeOffset? to = toEl.ValueKind == JsonValueKind.Null ? null : toEl.GetDateTimeOffset();
        var reconstructed = new EffectivePeriod(from, to);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void EffectivePeriod_Serialization_IsStable()
    {
        var p = new EffectivePeriod(_utc, _utcEnd);
        Assert.Equal(JsonSerializer.Serialize(p), JsonSerializer.Serialize(p));
    }

    // -------------------------------------------------------------------------
    // Timezone drift (topic 20 — timezone drift validation)
    // -------------------------------------------------------------------------

    [Fact]
    public void TimePoint_VariousUtcOffsets_AllNormalizeToSameUtcInstant()
    {
        // The same instant expressed in different UTC offsets must produce the
        // same stored timestamp after construction.
        var offsets = new[] { -12, -8, -5, 0, 3, 5, 8, 12 };
        var utcInstant = _utc;
        foreach (var hours in offsets)
        {
            var withOffset = new DateTimeOffset(
                utcInstant.UtcDateTime + TimeSpan.FromHours(hours),
                TimeSpan.FromHours(hours));
            var tp = new TimePoint(withOffset);
            Assert.Equal(TimeSpan.Zero, tp.Timestamp.Offset);
            Assert.Equal(utcInstant, tp.Timestamp);
        }
    }

    [Fact]
    public void TimePoint_NonUtcInput_RoundtripPreservesUtcInstant()
    {
        var original = new TimePoint(_nonUtcPlus2);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var timestamp = doc.RootElement.GetProperty("Timestamp").GetDateTimeOffset();
        var reconstructed = new TimePoint(timestamp);
        Assert.True(original.IsSameInstant(reconstructed));
        Assert.Equal(TimeSpan.Zero, reconstructed.Timestamp.Offset);
    }

    [Fact]
    public void TimeRange_NonUtcBoundaries_RoundtripPreservesUtcInstants()
    {
        var nonUtcStart = new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.FromHours(5));
        var nonUtcEnd = new DateTimeOffset(2025, 3, 1, 18, 0, 0, TimeSpan.FromHours(5));
        var original = new TimeRange(nonUtcStart, nonUtcEnd);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var start = doc.RootElement.GetProperty("Start").GetDateTimeOffset();
        var end = doc.RootElement.GetProperty("End").GetDateTimeOffset();
        var reconstructed = new TimeRange(start, end);
        Assert.Equal(original, reconstructed);
        Assert.Equal(TimeSpan.Zero, reconstructed.Start.Offset);
        Assert.Equal(TimeSpan.Zero, reconstructed.End.Offset);
    }

    [Fact]
    public void EffectivePeriod_NonUtcBounds_NormalizeToUtcOnStorage()
    {
        var nonUtcFrom = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.FromHours(8));
        var nonUtcTo = new DateTimeOffset(2025, 1, 1, 8, 0, 0, TimeSpan.FromHours(8));
        var p = new EffectivePeriod(nonUtcFrom, nonUtcTo);
        Assert.Equal(TimeSpan.Zero, p.EffectiveFrom!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, p.EffectiveTo!.Value.Offset);
    }

    // -------------------------------------------------------------------------
    // Precision preservation (topic 20 — precision loss validation)
    // -------------------------------------------------------------------------

    [Fact]
    public void TimePoint_PrecisionPreserved_ToTickLevel()
    {
        var withTicks = _utc.AddTicks(99999);
        var tp = new TimePoint(withTicks);
        Assert.Equal(withTicks.Ticks, tp.Timestamp.Ticks);
    }

    [Fact]
    public void TimeRange_SubMillisecondDuration_IsPreserved()
    {
        var start = _utc;
        var end = start.AddTicks(1);
        var r = new TimeRange(start, end);
        Assert.Equal(TimeSpan.FromTicks(1), r.Duration);
    }

    [Fact]
    public void TimePoint_Roundtrip_PreservesTickPrecision()
    {
        var withTicks = _utc.AddTicks(12345678);
        var original = new TimePoint(withTicks);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var timestamp = doc.RootElement.GetProperty("Timestamp").GetDateTimeOffset();
        var reconstructed = new TimePoint(timestamp);
        Assert.Equal(original.Timestamp.Ticks, reconstructed.Timestamp.Ticks);
    }

    // -------------------------------------------------------------------------
    // Cross-type temporal consistency
    // -------------------------------------------------------------------------

    [Fact]
    public void TimeRange_And_TimeWindow_SameBounds_ProduceConsistentContainment()
    {
        var mid = _utc.AddHours(3);
        var range = new TimeRange(_utc, _utcEnd);
        var window = new TimeWindow(_utc, _utcEnd);

        // TimeRange is [start, end] inclusive; TimeWindow is [start, end) exclusive
        Assert.True(range.Contains(_utcEnd));
        Assert.False(window.Contains(_utcEnd));
        Assert.True(range.Contains(mid));
        Assert.True(window.Contains(mid));
    }

    [Fact]
    public void TimeWindow_ToTimeRange_RoundtripIsConsistent()
    {
        var original = new TimeWindow(_utc, _utcEnd);
        var asRange = original.ToTimeRange();
        Assert.Equal(original.Start, asRange.Start);
        Assert.Equal(original.End!.Value, asRange.End);
    }
}
