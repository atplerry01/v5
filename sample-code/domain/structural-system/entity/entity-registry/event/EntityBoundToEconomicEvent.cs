namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityBoundToEconomicEvent(Guid EntityId, Guid EconomicAccountId) : DomainEvent;
