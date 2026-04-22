using System.Text.Json;
using Whycespace.Domain.CoreSystem.Ordering.OrderingKey;
using Whycespace.Domain.CoreSystem.Ordering.OrderingRule;
using Whycespace.Domain.CoreSystem.Ordering.Sequence;

namespace Whycespace.Tests.Unit.CoreSystem.Ordering;

/// <summary>
/// E12 — Serialization roundtrip and ordering-stability tests for all three
/// ordering primitives. Covers topics 7 (ordering invariants), 13 (persistence),
/// 19 (test certification), and 20 (resilience) of core-system.md.
/// </summary>
public sealed class CoreSystemOrderingSerializationTests
{
    // -------------------------------------------------------------------------
    // Sequence — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void Sequence_Serializes_WithLongValueProperty()
    {
        var s = new Sequence(42);
        var json = JsonSerializer.Serialize(s);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(42L, doc.RootElement.GetProperty("Value").GetInt64());
    }

    [Fact]
    public void Sequence_Roundtrip_PreservesValue()
    {
        var original = new Sequence(9_999_999);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetInt64();
        var reconstructed = new Sequence(value);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void Sequence_Zero_Roundtrip()
    {
        var json = JsonSerializer.Serialize(Sequence.Zero);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetInt64();
        Assert.Equal(0L, value);
        Assert.Equal(Sequence.Zero, new Sequence(value));
    }

    [Fact]
    public void Sequence_Serialization_IsStable()
    {
        var s = new Sequence(100);
        Assert.Equal(JsonSerializer.Serialize(s), JsonSerializer.Serialize(s));
    }

    // -------------------------------------------------------------------------
    // SequenceRange — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void SequenceRange_Roundtrip_PreservesStartAndEndAndCount()
    {
        var original = new SequenceRange(new Sequence(10), new Sequence(20));
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var startVal = doc.RootElement.GetProperty("Start").GetProperty("Value").GetInt64();
        var endVal = doc.RootElement.GetProperty("End").GetProperty("Value").GetInt64();
        var reconstructed = new SequenceRange(new Sequence(startVal), new Sequence(endVal));
        Assert.Equal(original, reconstructed);
        Assert.Equal(11L, reconstructed.Count);
    }

    [Fact]
    public void SequenceRange_SingleElement_Roundtrip()
    {
        var original = new SequenceRange(new Sequence(5), new Sequence(5));
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var start = doc.RootElement.GetProperty("Start").GetProperty("Value").GetInt64();
        var end = doc.RootElement.GetProperty("End").GetProperty("Value").GetInt64();
        Assert.Equal(5L, start);
        Assert.Equal(5L, end);
        var reconstructed = new SequenceRange(new Sequence(start), new Sequence(end));
        Assert.Equal(1L, reconstructed.Count);
    }

    // -------------------------------------------------------------------------
    // OrderingKey — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderingKey_Serializes_WithStringValueProperty()
    {
        var k = new OrderingKey("created-at");
        var json = JsonSerializer.Serialize(k);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("created-at", doc.RootElement.GetProperty("Value").GetString());
    }

    [Fact]
    public void OrderingKey_Roundtrip_PreservesValue()
    {
        var original = new OrderingKey("settlement-date");
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetString()!;
        var reconstructed = new OrderingKey(value);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void OrderingKey_Serialization_IsStable()
    {
        var k = new OrderingKey("score");
        Assert.Equal(JsonSerializer.Serialize(k), JsonSerializer.Serialize(k));
    }

    [Fact]
    public void OrderingKey_RoundtripPreservesOrdinalCaseSensitivity()
    {
        var lower = new OrderingKey("key");
        var upper = new OrderingKey("KEY");
        var lowerJson = JsonSerializer.Serialize(lower);
        var upperJson = JsonSerializer.Serialize(upper);
        using var lDoc = JsonDocument.Parse(lowerJson);
        using var uDoc = JsonDocument.Parse(upperJson);
        var lRecon = new OrderingKey(lDoc.RootElement.GetProperty("Value").GetString()!);
        var uRecon = new OrderingKey(uDoc.RootElement.GetProperty("Value").GetString()!);
        Assert.True(lRecon.CompareTo(uRecon) > 0); // ordinal: lowercase > uppercase
    }

    // -------------------------------------------------------------------------
    // Ordering stability (topic 7 — deterministic ordering, topic 20 — stability)
    // -------------------------------------------------------------------------

    [Fact]
    public void Sequence_CompareTo_IsStable_AcrossRepeatedCalls()
    {
        var a = new Sequence(5);
        var b = new Sequence(10);
        var results = Enumerable.Range(0, 20).Select(_ => a.CompareTo(b)).ToList();
        Assert.True(results.All(r => r < 0), "a < b must hold on every invocation");
    }

    [Fact]
    public void Sequence_Next_IsMonotonic_UnderReplay()
    {
        // The same Sequence must always produce the same Next value.
        var s = new Sequence(7);
        var calls = Enumerable.Range(0, 20).Select(_ => s.Next()).ToList();
        Assert.True(calls.All(n => n.Value == 8L));
    }

    [Fact]
    public void OrderingKey_CompareTo_IsStable_AcrossRepeatedCalls()
    {
        var x = new OrderingKey("alpha");
        var y = new OrderingKey("beta");
        var results = Enumerable.Range(0, 20).Select(_ => x.CompareTo(y)).ToList();
        Assert.True(results.All(r => r < 0), "x < y must hold on every invocation");
    }

    [Fact]
    public void OrderingRule_Compare_IsStable_AcrossRepeatedCalls()
    {
        var rule = OrderingRule.AscendingBy(new OrderingKey("field"));
        var a = new OrderingKey("alpha");
        var b = new OrderingKey("beta");
        var first = rule.Compare(a, b);
        var results = Enumerable.Range(0, 20).Select(_ => rule.Compare(a, b)).ToList();
        Assert.True(results.All(r => r == first));
        Assert.True(first < 0);
    }

    [Fact]
    public void OrderingKey_Antisymmetry_Holds()
    {
        var x = new OrderingKey("apple");
        var y = new OrderingKey("banana");
        var xy = x.CompareTo(y);
        var yx = y.CompareTo(x);
        Assert.True(xy < 0);
        Assert.True(yx > 0);
        Assert.True(xy * yx < 0); // opposite signs
    }

    [Fact]
    public void OrderingKey_Transitivity_Holds()
    {
        var a = new OrderingKey("a");
        var b = new OrderingKey("b");
        var c = new OrderingKey("c");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(c) < 0);
        Assert.True(a.CompareTo(c) < 0);
    }

    [Fact]
    public void Sequence_Antisymmetry_Holds()
    {
        var a = new Sequence(1);
        var b = new Sequence(2);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void Sequence_Transitivity_Holds()
    {
        var a = new Sequence(1);
        var b = new Sequence(5);
        var c = new Sequence(10);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(c) < 0);
        Assert.True(a.CompareTo(c) < 0);
    }

    // -------------------------------------------------------------------------
    // OrderingRule — structural serialization
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderingRule_Ascending_Serializes_WithKeyDirectionAndNullTieBreaker()
    {
        var rule = OrderingRule.AscendingBy(new OrderingKey("created-at"));
        var json = JsonSerializer.Serialize(rule);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("created-at", doc.RootElement.GetProperty("Key").GetProperty("Value").GetString());
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("TieBreaker").ValueKind);
    }

    [Fact]
    public void OrderingRule_Ascending_Serialization_IsStable()
    {
        var rule = OrderingRule.AscendingBy(new OrderingKey("field"));
        Assert.Equal(JsonSerializer.Serialize(rule), JsonSerializer.Serialize(rule));
    }

    [Fact]
    public void OrderingRule_WithTieBreaker_Serializes_NestedTieBreakerProperty()
    {
        var primary = OrderingRule.AscendingBy(new OrderingKey("category"));
        var rule = primary.WithTieBreaker(OrderingRule.DescendingBy(new OrderingKey("name")));
        var json = JsonSerializer.Serialize(rule);
        using var doc = JsonDocument.Parse(json);
        var tieBreaker = doc.RootElement.GetProperty("TieBreaker");
        Assert.NotEqual(JsonValueKind.Null, tieBreaker.ValueKind);
        Assert.Equal("name", tieBreaker.GetProperty("Key").GetProperty("Value").GetString());
    }
}
