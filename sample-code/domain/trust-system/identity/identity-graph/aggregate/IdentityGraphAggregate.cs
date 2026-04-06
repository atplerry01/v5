using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphAggregate : AggregateRoot
{
    public IdentityGraphId GraphId { get; private set; } = null!;
    public Guid PrimaryIdentityId { get; private set; }
    public IdentityGraphStatus Status { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<IdentityLink> _links = [];
    public IReadOnlyList<IdentityLink> Links => _links.AsReadOnly();

    private IdentityGraphAggregate() { }

    public static IdentityGraphAggregate Create(Guid primaryIdentityId, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(primaryIdentityId);

        var graph = new IdentityGraphAggregate
        {
            GraphId = IdentityGraphId.FromSeed($"IdentityGraph:{primaryIdentityId}"),
            PrimaryIdentityId = primaryIdentityId,
            Status = IdentityGraphStatus.Active,
            CreatedAt = timestamp
        };

        graph.Id = graph.GraphId.Value;
        graph.RaiseDomainEvent(new IdentityGraphCreatedEvent(
            graph.GraphId.Value, primaryIdentityId));
        return graph;
    }

    public IdentityLink CreateLink(Guid sourceIdentityId, Guid targetIdentityId, LinkType linkType, LinkStrength strength, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(sourceIdentityId);
        Guard.AgainstDefault(targetIdentityId);
        Guard.AgainstNull(linkType);
        Guard.AgainstNull(strength);

        EnsureInvariant(
            Status == IdentityGraphStatus.Active,
            "GRAPH_MUST_BE_ACTIVE",
            "Cannot modify a closed graph.");

        EnsureInvariant(
            sourceIdentityId != targetIdentityId,
            "SELF_LINK_NOT_ALLOWED",
            "Cannot link an identity to itself.");

        EnsureInvariant(
            !_links.Any(l => l.SourceIdentityId == sourceIdentityId
                          && l.TargetIdentityId == targetIdentityId
                          && l.LinkType == linkType
                          && l.IsActive),
            "LINK_ALREADY_EXISTS",
            "This link already exists.");

        var link = IdentityLink.Create(GraphId.Value, sourceIdentityId, targetIdentityId, linkType, strength, timestamp);
        _links.Add(link);

        RaiseDomainEvent(new IdentityLinkedEvent(
            GraphId.Value, sourceIdentityId, targetIdentityId, linkType.Value, strength.Value));
        return link;
    }

    public void RemoveLink(Guid sourceIdentityId, Guid targetIdentityId, LinkType linkType)
    {
        var link = _links.FirstOrDefault(l =>
            l.SourceIdentityId == sourceIdentityId
            && l.TargetIdentityId == targetIdentityId
            && l.LinkType == linkType
            && l.IsActive);

        EnsureInvariant(
            link is not null,
            "LINK_NOT_FOUND",
            "No active link found matching criteria.");

        link!.Deactivate();

        RaiseDomainEvent(new IdentityUnlinkedEvent(
            GraphId.Value, sourceIdentityId, targetIdentityId, linkType.Value));
    }

    public void Merge(IdentityGraphAggregate other, DateTimeOffset timestamp)
    {
        Guard.AgainstNull(other);
        EnsureInvariant(
            Status == IdentityGraphStatus.Active && other.Status == IdentityGraphStatus.Active,
            "BOTH_GRAPHS_MUST_BE_ACTIVE",
            "Both graphs must be active to merge.");

        foreach (var link in other.Links.Where(l => l.IsActive))
        {
            if (!_links.Any(l => l.SourceIdentityId == link.SourceIdentityId
                              && l.TargetIdentityId == link.TargetIdentityId
                              && l.LinkType == link.LinkType
                              && l.IsActive))
            {
                var newLink = IdentityLink.Create(
                    GraphId.Value, link.SourceIdentityId, link.TargetIdentityId,
                    link.LinkType, link.Strength, timestamp);
                _links.Add(newLink);
            }
        }

        RaiseDomainEvent(new IdentityGraphMergedEvent(
            GraphId.Value, other.GraphId.Value));
    }

    public void Close()
    {
        EnsureNotTerminal(Status, s => s == IdentityGraphStatus.Closed, "Close");
        Status = IdentityGraphStatus.Closed;
        RaiseDomainEvent(new IdentityGraphClosedEvent(GraphId.Value));
    }
}
