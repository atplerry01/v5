using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed record KanbanBoardCreatedEvent(AggregateId AggregateId, string Name) : DomainEvent;
