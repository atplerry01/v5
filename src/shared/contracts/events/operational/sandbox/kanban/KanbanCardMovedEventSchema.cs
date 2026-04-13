namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanCardMovedEventSchema(Guid AggregateId, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition);
