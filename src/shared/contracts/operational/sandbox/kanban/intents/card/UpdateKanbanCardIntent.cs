namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record UpdateKanbanCardIntent(Guid CardId, Guid BoardId, string Title, string Description, string UserId);
