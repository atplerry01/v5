namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanCardCompletedEventSchema(Guid AggregateId, Guid CardId);
