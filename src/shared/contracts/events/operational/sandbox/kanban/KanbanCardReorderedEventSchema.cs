namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanCardReorderedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, int NewPosition);
