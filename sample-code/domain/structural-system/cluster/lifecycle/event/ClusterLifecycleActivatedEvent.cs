using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record ClusterLifecycleActivatedEvent(
    Guid ClusterId,
    DateTimeOffset ActivatedAt
) : DomainEvent;
