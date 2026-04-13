namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record UpdateKanbanCardCommand(Guid Id, Guid CardId, string Title, string Description);
