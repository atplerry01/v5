using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityCapabilityAssignedEvent(Guid EntityId, string Capability) : DomainEvent;
