namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityBoundToIdentityEvent(Guid EntityId, Guid IdentityId) : DomainEvent;
