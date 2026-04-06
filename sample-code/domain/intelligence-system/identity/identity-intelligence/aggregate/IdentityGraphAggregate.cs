namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Intelligence graph aggregate — read-only intelligence model of identity relationships.
/// This is NOT the domain IdentityGraph (core-access-trust). This is the intelligence projection
/// of relationships across identities, devices, sessions, organizations, and services.
/// </summary>
public sealed class IdentityGraphAggregate : AggregateRoot
{
    private readonly Dictionary<string, IdentityNode> _identityNodes = new();
    private readonly Dictionary<string, DeviceNode> _deviceNodes = new();
    private readonly Dictionary<string, SessionNode> _sessionNodes = new();
    private readonly Dictionary<string, ServiceNode> _serviceNodes = new();
    private readonly List<RelationshipEdge> _edges = [];

    public IReadOnlyDictionary<string, IdentityNode> IdentityNodes => _identityNodes;
    public IReadOnlyDictionary<string, DeviceNode> DeviceNodes => _deviceNodes;
    public IReadOnlyDictionary<string, SessionNode> SessionNodes => _sessionNodes;
    public IReadOnlyDictionary<string, ServiceNode> ServiceNodes => _serviceNodes;
    public IReadOnlyList<RelationshipEdge> Edges => _edges.AsReadOnly();

    public void AddIdentityNode(IdentityNode node)
    {
        Guard.AgainstNull(node);
        _identityNodes[node.IdentityId] = node;
        RaiseDomainEvent(new GraphUpdatedEvent(
            Id, "node_added", "identity", node.IdentityId));
    }

    public void AddDeviceNode(DeviceNode node)
    {
        Guard.AgainstNull(node);
        _deviceNodes[node.DeviceId] = node;
        RaiseDomainEvent(new GraphUpdatedEvent(
            Id, "node_added", "device", node.DeviceId));
    }

    public void AddSessionNode(SessionNode node)
    {
        Guard.AgainstNull(node);
        _sessionNodes[node.SessionId] = node;
    }

    public void AddServiceNode(ServiceNode node)
    {
        Guard.AgainstNull(node);
        _serviceNodes[node.ServiceId] = node;
    }

    public void AddEdge(RelationshipEdge edge)
    {
        Guard.AgainstNull(edge);
        _edges.Add(edge);
    }

    public IReadOnlyList<RelationshipEdge> GetEdgesForNode(string nodeId)
    {
        return _edges
            .Where(e => e.IsActive && (e.SourceNodeId == nodeId || e.TargetNodeId == nodeId))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<string> GetConnectedNodeIds(string nodeId)
    {
        return _edges
            .Where(e => e.IsActive && (e.SourceNodeId == nodeId || e.TargetNodeId == nodeId))
            .Select(e => e.SourceNodeId == nodeId ? e.TargetNodeId : e.SourceNodeId)
            .Distinct()
            .ToList()
            .AsReadOnly();
    }
}
