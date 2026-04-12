namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CreateKanbanListCommand(Guid Id, Guid ListId, string Name, int Position);
