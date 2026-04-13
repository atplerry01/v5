namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CreateKanbanListIntent(Guid BoardId, string Name, int Position, string UserId);
