using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record SubClusterSpvAddedEvent(
    Guid SubClusterId,
    Guid SpvId) : DomainEvent;
