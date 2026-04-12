namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record KanbanListDto
{
    public Guid ListId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public IReadOnlyList<KanbanCardDto> Cards { get; init; } = [];
}
