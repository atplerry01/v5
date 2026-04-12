using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanCardCompletedEvent(
    AggregateId AggregateId,
    KanbanCardId CardId) : DomainEvent;
