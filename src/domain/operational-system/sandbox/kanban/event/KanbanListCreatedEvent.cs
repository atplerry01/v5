using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanListCreatedEvent(
    AggregateId AggregateId,
    KanbanListId ListId,
    string Name,
    KanbanPosition Position) : DomainEvent;
