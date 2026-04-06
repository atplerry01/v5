namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityLinkedEvent(Guid EntityId, Guid ParentId) : DomainEvent;
