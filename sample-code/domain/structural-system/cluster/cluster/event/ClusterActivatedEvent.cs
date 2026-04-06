using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed record ClusterActivatedEvent(Guid ClusterId) : DomainEvent;
