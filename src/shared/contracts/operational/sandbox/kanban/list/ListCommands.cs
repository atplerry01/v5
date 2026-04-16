using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

public sealed record CreateKanbanListCommand(Guid Id, Guid ListId, string Name, int Position) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record CreateKanbanListIntent(Guid BoardId, string Name, int Position, string UserId);
