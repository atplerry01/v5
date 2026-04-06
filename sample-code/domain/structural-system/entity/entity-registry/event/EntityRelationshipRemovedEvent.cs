namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityRelationshipRemovedEvent(
    Guid FromEntityId,
    Guid ToEntityId,
    string RelationshipType) : DomainEvent;
