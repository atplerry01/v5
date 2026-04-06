namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvReactivatedEvent(
    Guid SpvId) : DomainEvent;
