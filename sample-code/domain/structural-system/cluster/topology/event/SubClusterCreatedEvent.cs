using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record SubClusterCreatedEvent(Guid SubClusterId) : DomainEvent;
