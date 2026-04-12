namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanCardReorderedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, int NewPosition);
