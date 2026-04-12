using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanCardCreatedEvent(
    AggregateId AggregateId,
    KanbanCardId CardId,
    KanbanListId ListId,
    string Title,
    string Description,
    KanbanPosition Position,
    KanbanPriority? Priority) : DomainEvent;
