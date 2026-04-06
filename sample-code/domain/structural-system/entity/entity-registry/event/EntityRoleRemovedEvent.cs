namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityRoleRemovedEvent(Guid EntityId, string Role) : DomainEvent;
