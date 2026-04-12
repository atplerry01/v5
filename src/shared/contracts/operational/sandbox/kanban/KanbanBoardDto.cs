namespace Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

public sealed record KanbanBoardDto
{
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<KanbanListDto> Lists { get; init; } = [];
}
