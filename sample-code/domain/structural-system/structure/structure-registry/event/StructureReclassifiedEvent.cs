namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureReclassifiedEvent(
    Guid StructureId,
    string OldType,
    string NewType) : DomainEvent;
