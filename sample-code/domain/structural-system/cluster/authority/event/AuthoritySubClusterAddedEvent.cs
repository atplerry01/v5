using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthoritySubClusterAddedEvent(
    Guid AuthorityId,
    Guid SubClusterId) : DomainEvent;
