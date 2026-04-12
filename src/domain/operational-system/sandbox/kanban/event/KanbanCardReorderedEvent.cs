using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanCardReorderedEvent(
    AggregateId AggregateId,
    KanbanCardId CardId,
    KanbanListId ListId,
    KanbanPosition NewPosition) : DomainEvent;
