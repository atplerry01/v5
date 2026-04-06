namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

/// <summary>Topic: whyce.identity.identity-graph.merged</summary>
public sealed record IdentityGraphMergedEvent(
    Guid TargetGraphId,
    Guid SourceGraphId) : DomainEvent;
