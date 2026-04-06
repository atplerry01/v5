using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityRoleAssignedEvent(Guid EntityId, string Role) : DomainEvent;
