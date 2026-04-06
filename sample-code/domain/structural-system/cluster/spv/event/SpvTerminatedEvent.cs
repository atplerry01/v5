namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvTerminatedEvent(
    Guid SpvId,
    string Reason) : DomainEvent;
