namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityReparentedEvent(Guid EntityId, Guid NewParentId) : DomainEvent;
