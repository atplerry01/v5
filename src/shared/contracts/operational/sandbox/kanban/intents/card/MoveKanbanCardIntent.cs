namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record MoveKanbanCardIntent(Guid CardId, Guid BoardId, Guid FromListId, Guid ToListId, int NewPosition, string UserId);
