namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

/// <summary>Topic: whyce.identity.identity-graph.closed</summary>
public sealed record IdentityGraphClosedEvent(Guid GraphId) : DomainEvent;
