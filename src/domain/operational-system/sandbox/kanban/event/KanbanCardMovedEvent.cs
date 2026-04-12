using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanCardMovedEvent(
    AggregateId AggregateId,
    KanbanCardId CardId,
    KanbanListId FromListId,
    KanbanListId ToListId,
    KanbanPosition NewPosition) : DomainEvent;
