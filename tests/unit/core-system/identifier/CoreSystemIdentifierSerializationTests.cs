using System.Text.Json;
using Whycespace.Domain.CoreSystem.Identifier.CausationId;
using Whycespace.Domain.CoreSystem.Identifier.CorrelationId;
using Whycespace.Domain.CoreSystem.Identifier.EntityReference;
using Whycespace.Domain.CoreSystem.Identifier.GlobalIdentifier;

namespace Whycespace.Tests.Unit.CoreSystem.Identifier;

/// <summary>
/// E12 — Serialization, propagation, and collision-resistance tests for
/// all four identifier primitives. Covers topics 13 (persistence), 14 (messaging),
/// 16 (API), 19 (test certification), and 20 (resilience) of core-system.md.
/// </summary>
public sealed class CoreSystemIdentifierSerializationTests
{
    private static readonly string _hex64a = new string('a', 64);
    private static readonly string _hex64b = new string('b', 64);
    private const string _entityType = "economic-system/capital/vault";

    // -------------------------------------------------------------------------
    // CorrelationId — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void CorrelationId_Serializes_ToJsonObjectWithValueProperty()
    {
        var id = new CorrelationId(_hex64a);
        var json = JsonSerializer.Serialize(id);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(_hex64a, doc.RootElement.GetProperty("Value").GetString());
    }

    [Fact]
    public void CorrelationId_Roundtrip_PreservesValue()
    {
        var original = new CorrelationId(_hex64a);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetString()!;
        var reconstructed = new CorrelationId(value);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void CorrelationId_Serialization_IsStable_SameInputSameOutput()
    {
        var id = new CorrelationId(_hex64a);
        var json1 = JsonSerializer.Serialize(id);
        var json2 = JsonSerializer.Serialize(id);
        Assert.Equal(json1, json2);
    }

    [Fact]
    public void CorrelationId_SerializedValue_IsHumanReadable()
    {
        var id = new CorrelationId(_hex64a);
        var json = JsonSerializer.Serialize(id);
        Assert.Contains(_hex64a, json);
    }

    // -------------------------------------------------------------------------
    // CausationId — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void CausationId_Roundtrip_PreservesValue()
    {
        var original = new CausationId(_hex64a);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetString()!;
        var reconstructed = new CausationId(value);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void CausationId_Serialization_IsStable()
    {
        var id = new CausationId(_hex64a);
        Assert.Equal(JsonSerializer.Serialize(id), JsonSerializer.Serialize(id));
    }

    [Fact]
    public void CausationId_SerializedValue_IsHumanReadable()
    {
        var id = new CausationId(_hex64b);
        var json = JsonSerializer.Serialize(id);
        Assert.Contains(_hex64b, json);
    }

    // -------------------------------------------------------------------------
    // GlobalIdentifier — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void GlobalIdentifier_Roundtrip_PreservesValue()
    {
        var original = new GlobalIdentifier(_hex64a);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("Value").GetString()!;
        var reconstructed = new GlobalIdentifier(value);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void GlobalIdentifier_Serialization_IsStable()
    {
        var id = new GlobalIdentifier(_hex64a);
        Assert.Equal(JsonSerializer.Serialize(id), JsonSerializer.Serialize(id));
    }

    // -------------------------------------------------------------------------
    // EntityReference — JSON roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void EntityReference_Serializes_WithBothProperties()
    {
        var original = new EntityReference(_hex64a, _entityType);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(_hex64a, doc.RootElement.GetProperty("IdentifierValue").GetString());
        Assert.Equal(_entityType, doc.RootElement.GetProperty("EntityType").GetString());
    }

    [Fact]
    public void EntityReference_Roundtrip_PreservesIdentifierAndType()
    {
        var original = new EntityReference(_hex64a, _entityType);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var idValue = doc.RootElement.GetProperty("IdentifierValue").GetString()!;
        var type = doc.RootElement.GetProperty("EntityType").GetString()!;
        var reconstructed = new EntityReference(idValue, type);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void EntityReference_Serialization_IsStable()
    {
        var ref1 = new EntityReference(_hex64a, _entityType);
        Assert.Equal(JsonSerializer.Serialize(ref1), JsonSerializer.Serialize(ref1));
    }

    [Fact]
    public void EntityReference_Roundtrip_AcrossDistinctEntityTypes()
    {
        var types = new[]
        {
            "trust-system/identity/credential",
            "economic-system/vault/account",
            "content-system/media/asset"
        };
        foreach (var type in types)
        {
            var original = new EntityReference(_hex64a, type);
            var json = JsonSerializer.Serialize(original);
            using var doc = JsonDocument.Parse(json);
            var reconstructed = new EntityReference(
                doc.RootElement.GetProperty("IdentifierValue").GetString()!,
                doc.RootElement.GetProperty("EntityType").GetString()!);
            Assert.Equal(original, reconstructed);
        }
    }

    // -------------------------------------------------------------------------
    // Correlation propagation (topic 19 — correlation propagation tests)
    // -------------------------------------------------------------------------

    [Fact]
    public void CorrelationId_PropagatesUnchangedAcrossMessageChain()
    {
        // The same root CorrelationId value must compare equal at every hop.
        var root = new CorrelationId(_hex64a);
        var hop1 = new CorrelationId(root.Value);
        var hop2 = new CorrelationId(hop1.Value);
        var hop3 = new CorrelationId(hop2.Value);
        Assert.Equal(root, hop3);
        Assert.Equal(root.Value, hop3.Value);
    }

    [Fact]
    public void CorrelationId_TwoDistinctRoots_ProduceDistinctChains()
    {
        var chain1Root = new CorrelationId(_hex64a);
        var chain2Root = new CorrelationId(_hex64b);
        var chain1Hop = new CorrelationId(chain1Root.Value);
        var chain2Hop = new CorrelationId(chain2Root.Value);
        Assert.NotEqual(chain1Hop, chain2Hop);
    }

    [Fact]
    public void CorrelationId_RoundtripThroughJsonPreservesChainEquality()
    {
        var root = new CorrelationId(_hex64a);
        var json = JsonSerializer.Serialize(root);
        using var doc = JsonDocument.Parse(json);
        var hop = new CorrelationId(doc.RootElement.GetProperty("Value").GetString()!);
        Assert.Equal(root, hop);
    }

    // -------------------------------------------------------------------------
    // Causation chain (topic 19 — causation chain tests)
    // -------------------------------------------------------------------------

    [Fact]
    public void CausationId_ChildCausationLinksToParentMessageId()
    {
        // The CausationId of a child message is derived from the parent's computed ID.
        var parentId = _hex64a;
        var childCausation = new CausationId(parentId);
        Assert.Equal(parentId, childCausation.Value);
    }

    [Fact]
    public void CausationId_ChainPreservesDirectCausation()
    {
        // A → B → C: each step records direct parent, not the root.
        var aId = _hex64a;
        var bCausation = new CausationId(aId);
        var bId = _hex64b;
        var cCausation = new CausationId(bId);
        Assert.NotEqual(bCausation, cCausation);  // C points to B, not A
        Assert.Equal(bId, cCausation.Value);
        Assert.Equal(aId, bCausation.Value);
    }

    [Fact]
    public void CausationId_RoundtripThroughJsonPreservesLineage()
    {
        var parentId = _hex64b;
        var original = new CausationId(parentId);
        var json = JsonSerializer.Serialize(original);
        using var doc = JsonDocument.Parse(json);
        var reconstructed = new CausationId(doc.RootElement.GetProperty("Value").GetString()!);
        Assert.Equal(original, reconstructed);
        Assert.Equal(parentId, reconstructed.Value);
    }

    // -------------------------------------------------------------------------
    // Identifier collision resistance (topic 20)
    // -------------------------------------------------------------------------

    [Fact]
    public void GlobalIdentifier_TenDistinctInputs_ProduceTenDistinctValues()
    {
        var hexBases = new[] { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3' };
        var ids = hexBases.Select(c => new GlobalIdentifier(new string(c, 64))).ToList();
        var distinctCount = ids.Select(i => i.Value).Distinct().Count();
        Assert.Equal(hexBases.Length, distinctCount);
    }

    [Fact]
    public void CorrelationId_TenDistinctPaddedInputs_ProduceTenDistinctIdentifiers()
    {
        var ids = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var twoChar = i.ToString("x2");
                return new CorrelationId(string.Concat(Enumerable.Repeat(twoChar, 32)));
            })
            .ToList();
        var distinctCount = ids.Select(x => x.Value).Distinct().Count();
        Assert.Equal(10, distinctCount);
    }

    [Fact]
    public void EntityReference_SameId_DifferentEntityTypes_AreNotSameEntity()
    {
        var ref1 = new EntityReference(_hex64a, "trust-system/identity/credential");
        var ref2 = new EntityReference(_hex64a, "economic-system/capital/vault");
        Assert.False(ref1.IsSameEntity(ref2));
        Assert.NotEqual(ref1, ref2);
    }

    [Fact]
    public void EntityReference_SameType_DifferentIds_AreNotSameEntity()
    {
        var ref1 = new EntityReference(_hex64a, _entityType);
        var ref2 = new EntityReference(_hex64b, _entityType);
        Assert.False(ref1.IsSameEntity(ref2));
    }
}
