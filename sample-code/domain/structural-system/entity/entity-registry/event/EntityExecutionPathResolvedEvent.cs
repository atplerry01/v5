namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed record EntityExecutionPathResolvedEvent(
    Guid StartEntityId,
    Guid EndEntityId,
    IReadOnlyCollection<Guid> Path) : DomainEvent;
