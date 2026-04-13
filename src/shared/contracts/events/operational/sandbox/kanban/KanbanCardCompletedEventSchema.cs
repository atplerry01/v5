namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanCardCompletedEventSchema(Guid AggregateId, Guid CardId);
