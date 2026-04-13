using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

public sealed record KanbanListReadModel
{
    public Guid ListId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public IReadOnlyList<KanbanCardReadModel> Cards { get; init; } = [];
}
