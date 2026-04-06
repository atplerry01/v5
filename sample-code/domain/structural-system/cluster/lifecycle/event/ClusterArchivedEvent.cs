using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record ClusterArchivedEvent(
    Guid ClusterId,
    DateTimeOffset ArchivedAt
) : DomainEvent;
