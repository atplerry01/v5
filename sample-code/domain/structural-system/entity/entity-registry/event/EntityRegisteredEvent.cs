namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityRegisteredEvent(
    Guid EntityId,
    string EntityType,
    string Name,
    Guid? ParentId) : DomainEvent;
