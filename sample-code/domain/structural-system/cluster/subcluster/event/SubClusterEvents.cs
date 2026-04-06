namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed record SubClusterCreatedEvent(
    Guid SubClusterId,
    Guid AuthorityId,
    string Name) : DomainEvent;

public sealed record SubClusterActivatedEvent(
    Guid SubClusterId) : DomainEvent;

public sealed record SubClusterDeactivatedEvent(
    Guid SubClusterId,
    string Reason) : DomainEvent;

public sealed record SubClusterSpvAddedEvent(
    Guid SubClusterId,
    Guid SpvId) : DomainEvent;

public sealed record SubClusterSpvRemovedEvent(
    Guid SubClusterId,
    Guid SpvId) : DomainEvent;
