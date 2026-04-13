using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;

public sealed record KanbanBoardReadModel
{
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<KanbanListReadModel> Lists { get; init; } = [];
}
