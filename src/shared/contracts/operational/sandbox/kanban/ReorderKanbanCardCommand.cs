namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record ReorderKanbanCardCommand(Guid Id, Guid CardId, Guid ListId, int NewPosition);
