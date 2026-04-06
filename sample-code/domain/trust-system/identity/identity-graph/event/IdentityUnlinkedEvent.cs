namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

/// <summary>Topic: whyce.identity.identity-graph.unlinked</summary>
public sealed record IdentityUnlinkedEvent(
    Guid GraphId,
    Guid SourceIdentityId,
    Guid TargetIdentityId,
    string LinkType) : DomainEvent;
