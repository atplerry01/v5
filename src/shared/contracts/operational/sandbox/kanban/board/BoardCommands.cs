namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Board;

public sealed record CreateKanbanBoardCommand(Guid Id, string Name, Guid ActorId);

public sealed record CreateKanbanBoardIntent(string Name, string UserId);
