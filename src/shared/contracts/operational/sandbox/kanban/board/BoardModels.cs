using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.List;

namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Board;

public sealed record KanbanBoardReadModel
{
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<KanbanListReadModel> Lists { get; init; } = [];
}
