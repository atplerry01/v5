namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityActivatedEvent(Guid EntityId) : DomainEvent;
