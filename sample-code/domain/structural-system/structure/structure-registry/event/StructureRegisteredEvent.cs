namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureRegisteredEvent(
    Guid StructureId,
    string StructureType,
    string Name) : DomainEvent;
