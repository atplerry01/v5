using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;

public sealed record CreateKanbanBoardCommand(Guid Id, string Name, Guid ActorId) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record CreateKanbanBoardIntent(string Name, string UserId);
