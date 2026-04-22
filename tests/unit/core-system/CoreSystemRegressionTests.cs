using Whycespace.Domain.CoreSystem.Identifier.CausationId;
using Whycespace.Domain.CoreSystem.Identifier.CorrelationId;
using Whycespace.Domain.CoreSystem.Identifier.EntityReference;
using Whycespace.Domain.CoreSystem.Identifier.GlobalIdentifier;
using Whycespace.Domain.CoreSystem.Ordering.OrderingKey;
using Whycespace.Domain.CoreSystem.Ordering.Sequence;
using Whycespace.Domain.CoreSystem.Temporal.EffectivePeriod;
using Whycespace.Domain.CoreSystem.Temporal.TimePoint;
using Whycespace.Domain.CoreSystem.Temporal.TimeRange;
using Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

namespace Whycespace.Tests.Unit.CoreSystem;

/// <summary>
/// E12 — Cross-cutting regression pack for all 11 core-system primitives.
/// Covers topic 20 (resilience: replay consistency, idempotency, boundary values,
/// UTC normalization idempotency) and topic 22 (completion criteria) of core-system.md.
/// </summary>
public sealed class CoreSystemRegressionTests
{
    private static readonly string _hex64a = new string('a', 64);
    private static readonly string _hex64b = new string('b', 64);
    private static readonly DateTimeOffset _utc = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _utcEnd = new(2025, 6, 1, 18, 0, 0, TimeSpan.Zero);

    // -------------------------------------------------------------------------
    // Replay consistency — same inputs → equal value, 20× iterations
    // (topic 20: replay consistency validation)
    // -------------------------------------------------------------------------

    [Fact]
    public void CorrelationId_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        var first = new CorrelationId(_hex64a);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new CorrelationId(_hex64a));
    }

    [Fact]
    public void CausationId_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        var first = new CausationId(_hex64b);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new CausationId(_hex64b));
    }

    [Fact]
    public void GlobalIdentifier_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        var first = new GlobalIdentifier(_hex64a);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new GlobalIdentifier(_hex64a));
    }

    [Fact]
    public void EntityReference_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        const string type = "economic-system/capital/vault";
        var first = new EntityReference(_hex64a, type);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new EntityReference(_hex64a, type));
    }

    [Fact]
    public void TimePoint_SameInput_AlwaysProducesSameInstant_AcrossReplays()
    {
        var first = new TimePoint(_utc);
        for (var i = 0; i < 20; i++)
            Assert.True(first.IsSameInstant(new TimePoint(_utc)));
    }

    [Fact]
    public void TimeRange_SameInput_AlwaysProducesSameRange_AcrossReplays()
    {
        var first = new TimeRange(_utc, _utcEnd);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new TimeRange(_utc, _utcEnd));
    }

    [Fact]
    public void TimeWindow_SameInput_AlwaysProducesSameWindow_AcrossReplays()
    {
        var first = new TimeWindow(_utc, _utcEnd);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new TimeWindow(_utc, _utcEnd));
    }

    [Fact]
    public void EffectivePeriod_SameInput_AlwaysProducesSamePeriod_AcrossReplays()
    {
        var first = new EffectivePeriod(_utc, _utcEnd);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new EffectivePeriod(_utc, _utcEnd));
    }

    [Fact]
    public void Sequence_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        var first = new Sequence(42);
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new Sequence(42));
    }

    [Fact]
    public void OrderingKey_SameInput_AlwaysProducesSameValue_AcrossReplays()
    {
        var first = new OrderingKey("field-name");
        for (var i = 0; i < 20; i++)
            Assert.Equal(first, new OrderingKey("field-name"));
    }

    // -------------------------------------------------------------------------
    // UTC normalization idempotency (topic 20: timezone drift validation)
    // -------------------------------------------------------------------------

    [Fact]
    public void TimePoint_UtcNormalization_IsIdempotent()
    {
        // Constructing from a non-UTC DateTimeOffset then re-constructing from
        // the stored (UTC) Timestamp must produce the same value.
        var nonUtc = new DateTimeOffset(2025, 6, 1, 15, 0, 0, TimeSpan.FromHours(3));
        var tp = new TimePoint(nonUtc);
        var tp2 = new TimePoint(tp.Timestamp); // already UTC
        Assert.Equal(tp.Timestamp, tp2.Timestamp);
    }

    [Fact]
    public void TimeRange_UtcNormalization_IsIdempotent()
    {
        var nonUtcStart = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.FromHours(5));
        var nonUtcEnd = new DateTimeOffset(2025, 1, 1, 18, 0, 0, TimeSpan.FromHours(5));
        var r = new TimeRange(nonUtcStart, nonUtcEnd);
        var r2 = new TimeRange(r.Start, r.End); // Start/End already in UTC
        Assert.Equal(r, r2);
    }

    [Fact]
    public void TimeWindow_UtcNormalization_IsIdempotent()
    {
        var nonUtc = new DateTimeOffset(2025, 3, 1, 9, 0, 0, TimeSpan.FromHours(-4));
        var w = new TimeWindow(nonUtc, nonUtc.AddHours(8));
        var w2 = new TimeWindow(w.Start, w.End);
        Assert.Equal(w, w2);
    }

    [Fact]
    public void EffectivePeriod_UtcNormalization_IsIdempotent()
    {
        var nonUtcFrom = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.FromHours(8));
        var nonUtcTo = new DateTimeOffset(2025, 1, 1, 8, 0, 0, TimeSpan.FromHours(8));
        var p = new EffectivePeriod(nonUtcFrom, nonUtcTo);
        var p2 = new EffectivePeriod(p.EffectiveFrom, p.EffectiveTo); // already UTC
        Assert.Equal(p, p2);
    }

    // -------------------------------------------------------------------------
    // Cross-domain type safety (topic 22 — no behavioral bleed between types)
    // -------------------------------------------------------------------------

    [Fact]
    public void CorrelationId_And_CausationId_AreDistinctTypes_SameValueNoConversion()
    {
        // They share the same format but are semantically distinct.
        var corr = new CorrelationId(_hex64a);
        var caus = new CausationId(_hex64a);
        Assert.Equal(_hex64a, corr.Value);
        Assert.Equal(_hex64a, caus.Value);
        Assert.IsType<CorrelationId>(corr);
        Assert.IsType<CausationId>(caus);
    }

    [Fact]
    public void GlobalIdentifier_And_CorrelationId_AreDistinctTypes()
    {
        var gid = new GlobalIdentifier(_hex64a);
        var cid = new CorrelationId(_hex64a);
        Assert.IsType<GlobalIdentifier>(gid);
        Assert.IsType<CorrelationId>(cid);
    }

    // -------------------------------------------------------------------------
    // Boundary values (topic 20: long-term stability, extreme inputs)
    // -------------------------------------------------------------------------

    [Fact]
    public void Sequence_MaxValue_IsCreated()
    {
        var large = new Sequence(long.MaxValue - 1);
        Assert.Equal(long.MaxValue - 1, large.Value);
    }

    [Fact]
    public void TimeRange_MinDuration_OneTick_IsCreated()
    {
        var start = _utc;
        var end = start.AddTicks(1);
        var r = new TimeRange(start, end);
        Assert.Equal(TimeSpan.FromTicks(1), r.Duration);
    }

    [Fact]
    public void TimePoint_AtMinValuePlusOneTick_IsCreated()
    {
        var earliest = DateTimeOffset.MinValue.AddTicks(1);
        var tp = new TimePoint(earliest);
        Assert.Equal(earliest.ToUniversalTime(), tp.Timestamp);
    }

    [Fact]
    public void TimePoint_AtMaxValue_IsCreated()
    {
        var latest = DateTimeOffset.MaxValue;
        var tp = new TimePoint(latest);
        Assert.Equal(latest.ToUniversalTime(), tp.Timestamp);
    }

    [Fact]
    public void EffectivePeriod_HalfBounded_FromOnly_RemainsActiveIndefinitely()
    {
        var p = new EffectivePeriod(_utc, null);
        Assert.True(p.IsActive(_utc));
        Assert.True(p.IsActive(_utc.AddYears(100)));
        Assert.False(p.IsActive(_utc.AddTicks(-1)));
    }

    [Fact]
    public void EffectivePeriod_HalfBounded_ToOnly_IsActiveBeforeTo()
    {
        var p = new EffectivePeriod(null, _utc);
        Assert.True(p.IsActive(_utc.AddTicks(-1)));
        Assert.False(p.IsActive(_utc));
        Assert.True(p.IsActive(DateTimeOffset.MinValue));
    }

    [Fact]
    public void TimeWindow_OpenWindow_ContainsAnyFuturePoint()
    {
        var w = new TimeWindow(_utc);
        Assert.True(w.IsOpen);
        Assert.True(w.Contains(_utc.AddYears(1000)));
        Assert.Null(w.Duration);
    }

    [Fact]
    public void EntityReference_AllLowerHexIds_AreAccepted()
    {
        var allChars = "0123456789abcdef";
        var value = string.Concat(Enumerable.Repeat(allChars, 4));
        var ref1 = new EntityReference(value, "core-system/identifier/global-identifier");
        Assert.Equal(value, ref1.IdentifierValue);
    }

    // -------------------------------------------------------------------------
    // Identifier format invariants (topic 4 — canonical reference formats)
    // -------------------------------------------------------------------------

    [Fact]
    public void CorrelationId_ToString_ProducesExactValue()
    {
        var id = new CorrelationId(_hex64a);
        Assert.Equal(_hex64a, id.ToString());
        Assert.Equal(64, id.ToString().Length);
    }

    [Fact]
    public void EntityReference_ToString_FollowsTypeColonIdFormat()
    {
        const string type = "trust-system/identity/credential";
        var ref1 = new EntityReference(_hex64a, type);
        Assert.Equal($"{type}:{_hex64a}", ref1.ToString());
    }

    [Fact]
    public void TimePoint_ToString_WithoutLabel_ProducesIso8601()
    {
        var tp = new TimePoint(_utc);
        var str = tp.ToString();
        Assert.Contains("+00:00", str);
        Assert.DoesNotContain("=", str);
    }

    [Fact]
    public void TimePoint_ToString_WithLabel_StartsWithLabel()
    {
        var tp = new TimePoint(_utc, "deadline");
        Assert.StartsWith("deadline=", tp.ToString());
    }

    [Fact]
    public void Sequence_ToString_IsDecimalRepresentation()
    {
        Assert.Equal("0", Sequence.Zero.ToString());
        Assert.Equal("42", new Sequence(42).ToString());
        Assert.Equal("1000000", new Sequence(1_000_000).ToString());
    }
}
