using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed record ClusterAuthorityAddedEvent(
    Guid ClusterId,
    Guid AuthorityId) : DomainEvent;
