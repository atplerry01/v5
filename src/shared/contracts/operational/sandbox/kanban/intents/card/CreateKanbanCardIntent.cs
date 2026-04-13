namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CreateKanbanCardIntent(Guid BoardId, Guid ListId, string Title, string Description, int Position, string UserId);
