namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record MoveKanbanCardCommand(Guid Id, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition);
