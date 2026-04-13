namespace Whycespace.Shared.Contracts.Events.Operational.Sandbox.Kanban.Board;

public sealed record KanbanBoardCreatedEventSchema(Guid AggregateId, string Name);
