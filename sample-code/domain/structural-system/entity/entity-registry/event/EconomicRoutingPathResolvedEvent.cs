namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EconomicRoutingPathResolvedEvent(
    Guid SourceEntityId,
    Guid TargetEntityId,
    IReadOnlyCollection<Guid> Path) : DomainEvent;
