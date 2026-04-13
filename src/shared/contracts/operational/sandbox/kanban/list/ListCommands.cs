namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban.List;

public sealed record CreateKanbanListCommand(Guid Id, Guid ListId, string Name, int Position);

public sealed record CreateKanbanListIntent(Guid BoardId, string Name, int Position, string UserId);
