namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureLinkedEvent(
    Guid FromId,
    Guid ToId,
    string RelationshipType) : DomainEvent;
