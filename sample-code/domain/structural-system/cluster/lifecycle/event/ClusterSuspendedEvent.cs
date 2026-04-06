using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record ClusterSuspendedEvent(
    Guid ClusterId,
    string Reason
) : DomainEvent;
