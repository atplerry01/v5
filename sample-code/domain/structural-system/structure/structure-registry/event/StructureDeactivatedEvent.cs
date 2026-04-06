namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureDeactivatedEvent(Guid StructureId) : DomainEvent;
