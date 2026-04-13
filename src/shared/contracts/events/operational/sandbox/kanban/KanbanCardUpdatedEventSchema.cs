namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanCardUpdatedEventSchema(Guid AggregateId, Guid CardId, string Title, string Description);
