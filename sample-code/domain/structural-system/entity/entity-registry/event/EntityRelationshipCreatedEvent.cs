namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityRelationshipCreatedEvent(
    Guid FromEntityId,
    Guid ToEntityId,
    string RelationshipType) : DomainEvent;
