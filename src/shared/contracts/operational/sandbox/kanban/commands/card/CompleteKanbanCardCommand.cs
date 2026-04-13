namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CompleteKanbanCardCommand(Guid Id, Guid CardId);
