namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyFederationGraphAggregate : AggregateRoot
{
    private PolicyFederationGraphAggregate() { }

    public Guid FederationId { get; private set; }
    public FederationGraphHash GraphHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<FederationNode> _nodes = [];
    public IReadOnlyList<FederationNode> Nodes => _nodes.AsReadOnly();

    private readonly List<FederationEdge> _edges = [];
    public IReadOnlyList<FederationEdge> Edges => _edges.AsReadOnly();

    public long FederationVersion { get; private set; }
    public string? PreviousGraphHash { get; private set; }
    public string? CreatedByContext { get; private set; }
    public string? ChangeReason { get; private set; }

    public static PolicyFederationGraphAggregate Create(
        Guid federationId,
        IReadOnlyList<FederationNode> nodes,
        IReadOnlyList<FederationEdge> edges,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        if (nodes.Count == 0)
            throw new ArgumentException("Federation graph must contain at least one node.", nameof(nodes));

        var graph = new PolicyFederationGraphAggregate
        {
            Id = federationId,
            FederationId = federationId,
            CreatedAt = timestamp
        };

        foreach (var node in nodes) graph._nodes.Add(node);
        foreach (var edge in edges) graph._edges.Add(edge);

        graph.GraphHash = FederationGraphHash.Compute(graph._nodes, graph._edges);

        graph.RaiseDomainEvent(new FederationGraphCreatedEvent(
            federationId,
            graph.GraphHash.Value,
            graph._nodes.Count,
            graph._edges.Count));

        return graph;
    }

    public void AddNode(FederationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        EnsureInvariant(
            !_nodes.Any(n => n.PolicyId == node.PolicyId && n.ClusterId == node.ClusterId),
            "UNIQUE_NODE",
            "Duplicate node in federation graph.");

        _nodes.Add(node);
        GraphHash = FederationGraphHash.Compute(_nodes, _edges);

        RaiseDomainEvent(new FederationGraphUpdatedEvent(FederationId, GraphHash.Value));
    }

    public void AddEdge(FederationEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);
        EnsureInvariant(
            _nodes.Any(n => n.PolicyId == edge.SourcePolicyId) &&
            _nodes.Any(n => n.PolicyId == edge.TargetPolicyId),
            "EDGE_NODE_EXISTS",
            "Edge references a policy not present in the graph.");

        _edges.Add(edge);
        GraphHash = FederationGraphHash.Compute(_nodes, _edges);

        RaiseDomainEvent(new FederationGraphUpdatedEvent(FederationId, GraphHash.Value));
    }

    public static PolicyFederationGraphAggregate CreateVersioned(
        Guid federationId,
        IReadOnlyList<FederationNode> nodes,
        IReadOnlyList<FederationEdge> edges,
        long federationVersion,
        string? previousGraphHash,
        string? createdByContext,
        string? changeReason,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        if (nodes.Count == 0)
            throw new ArgumentException("Federation graph must contain at least one node.", nameof(nodes));

        var graph = new PolicyFederationGraphAggregate
        {
            Id = federationId,
            FederationId = federationId,
            CreatedAt = timestamp,
            FederationVersion = federationVersion,
            PreviousGraphHash = previousGraphHash,
            CreatedByContext = createdByContext,
            ChangeReason = changeReason
        };

        foreach (var node in nodes) graph._nodes.Add(node);
        foreach (var edge in edges) graph._edges.Add(edge);

        graph.GraphHash = FederationGraphHash.Compute(graph._nodes, graph._edges);

        graph.RaiseDomainEvent(new FederationGraphVersionedEvent(
            federationId,
            graph.GraphHash.Value,
            federationVersion,
            previousGraphHash,
            changeReason));

        return graph;
    }
}
