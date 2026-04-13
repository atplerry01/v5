namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CompleteKanbanCardIntent(Guid CardId, Guid BoardId, string UserId);
