namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanCardUpdatedEventSchema(Guid AggregateId, Guid CardId, string Title, string Description);
