using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed class StructureAggregate : AggregateRoot
{
    private readonly Dictionary<Guid, StructureNode> _nodes = new();
    private readonly List<StructureRelationship> _relationships = new();

    public IReadOnlyCollection<StructureNode> Nodes => _nodes.Values.ToList().AsReadOnly();
    public IReadOnlyCollection<StructureRelationship> Relationships => _relationships.AsReadOnly();

    public StructureAggregate() { }

    // ============================================
    // FACTORY
    // ============================================

    public static StructureAggregate Create(Guid id)
    {
        Guard.AgainstDefault(id);

        var aggregate = new StructureAggregate { Id = id };
        return aggregate;
    }

    // ============================================
    // NODE OPERATIONS
    // ============================================

    public StructureNode RegisterNode(
        StructureType type,
        StructureName name,
        JurisdictionCode jurisdictionCode,
        string seed)
    {
        Guard.AgainstEmpty(seed);

        var structureId = StructureId.CreateDeterministic(seed);

        EnsureInvariant(
            !_nodes.ContainsKey(structureId.Value),
            "UNIQUE_NODE",
            $"Node already exists: {structureId.Value}");

        var node = StructureNode.Create(structureId, type, name, jurisdictionCode);
        _nodes.Add(structureId.Value, node);

        RaiseDomainEvent(new StructureRegisteredEvent(structureId.Value, type.Value, name.Value));

        return node;
    }

    public void ReclassifyNode(StructureId structureId, StructureType newType)
    {
        var node = GetNode(structureId);
        var oldType = node.Type;

        node.Reclassify(newType);

        RaiseDomainEvent(new StructureReclassifiedEvent(structureId.Value, oldType.Value, newType.Value));
    }

    public void DeactivateNode(StructureId structureId)
    {
        var node = GetNode(structureId);

        EnsureInvariant(
            node.Status != StructureStatus.Inactive,
            "NODE_NOT_INACTIVE",
            "Node is already inactive");

        node.Deactivate();

        RaiseDomainEvent(new StructureDeactivatedEvent(structureId.Value));
    }

    // ============================================
    // RELATIONSHIP OPERATIONS
    // ============================================

    public void LinkNodes(
        StructureId from,
        StructureId to,
        RelationshipType relationshipType)
    {
        var fromNode = GetNode(from);
        var toNode = GetNode(to);

        EnsureInvariant(
            ValidStructureRelationshipSpecification.IsSatisfied(fromNode.Type, toNode.Type, relationshipType),
            "VALID_RELATIONSHIP",
            $"Invalid relationship {relationshipType.Value} between {fromNode.Type.Value} and {toNode.Type.Value}");

        var exists = _relationships.Any(r => r.Matches(from, to, relationshipType));
        if (exists)
            return;

        var relationshipId = DeterministicIdHelper.FromSeed(
            $"{from.Value}:{to.Value}:{relationshipType.Value}");

        var relationship = StructureRelationship.Create(relationshipId, from, to, relationshipType);
        _relationships.Add(relationship);

        RaiseDomainEvent(new StructureLinkedEvent(from.Value, to.Value, relationshipType.Value));
    }

    // ============================================
    // QUERY
    // ============================================

    public StructureNode? FindNode(StructureId structureId)
    {
        _nodes.TryGetValue(structureId.Value, out var node);
        return node;
    }

    // ============================================
    // INTERNAL
    // ============================================

    private StructureNode GetNode(StructureId structureId)
    {
        if (!_nodes.TryGetValue(structureId.Value, out var node))
            throw new StructureNotFoundException(structureId);

        return node;
    }
}
