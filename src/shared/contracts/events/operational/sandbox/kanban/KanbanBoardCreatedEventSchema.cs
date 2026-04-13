namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanBoardCreatedEventSchema(Guid AggregateId, string Name);
