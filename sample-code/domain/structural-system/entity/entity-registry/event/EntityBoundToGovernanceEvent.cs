namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityBoundToGovernanceEvent(Guid EntityId, string GovernanceScope) : DomainEvent;
