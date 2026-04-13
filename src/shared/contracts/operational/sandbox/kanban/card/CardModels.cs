namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Card;

public sealed record KanbanCardReadModel
{
    public Guid CardId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Position { get; init; }
    public bool IsCompleted { get; init; }
}
