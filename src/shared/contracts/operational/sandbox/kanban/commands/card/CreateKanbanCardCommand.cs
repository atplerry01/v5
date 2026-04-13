namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CreateKanbanCardCommand(Guid Id, Guid CardId, Guid ListId, string Title, string Description, int Position);
