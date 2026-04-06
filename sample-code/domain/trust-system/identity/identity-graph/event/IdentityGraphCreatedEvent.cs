namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

/// <summary>Topic: whyce.identity.identity-graph.created</summary>
public sealed record IdentityGraphCreatedEvent(
    Guid GraphId,
    Guid PrimaryIdentityId) : DomainEvent;
