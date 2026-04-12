namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanBoardCreatedEventSchema(Guid AggregateId, string Name);
