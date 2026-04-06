namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

/// <summary>Topic: whyce.identity.identity-graph.linked</summary>
public sealed record IdentityLinkedEvent(
    Guid GraphId,
    Guid SourceIdentityId,
    Guid TargetIdentityId,
    string LinkType,
    string Strength) : DomainEvent;
