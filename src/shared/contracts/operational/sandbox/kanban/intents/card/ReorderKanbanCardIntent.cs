namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record ReorderKanbanCardIntent(Guid CardId, Guid BoardId, Guid ListId, int NewPosition, string UserId);
