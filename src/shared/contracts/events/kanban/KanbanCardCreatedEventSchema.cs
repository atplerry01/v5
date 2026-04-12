namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanCardCreatedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, string Title, string Description, int Position, string? Priority);
