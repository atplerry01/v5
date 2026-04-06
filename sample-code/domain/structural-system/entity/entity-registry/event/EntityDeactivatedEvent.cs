namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityDeactivatedEvent(Guid EntityId) : DomainEvent;
