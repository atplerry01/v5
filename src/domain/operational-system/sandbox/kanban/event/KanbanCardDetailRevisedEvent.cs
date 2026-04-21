using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanCardDetailRevisedEvent(
    AggregateId AggregateId,
    KanbanCardId CardId,
    KanbanCardTitle Title,
    DocumentRef Description) : DomainEvent;
