namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record CreateKanbanBoardCommand(Guid Id, string Name, Guid ActorId);
