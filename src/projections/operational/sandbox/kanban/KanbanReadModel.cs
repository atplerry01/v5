using System.Text.Json.Serialization;

namespace Whyce.Projections.Operational.Sandbox.Kanban;

public sealed record KanbanBoardReadModel
{
    [JsonPropertyName("boardId")]
    public Guid BoardId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("lists")]
    public List<KanbanListReadModel> Lists { get; init; } = [];
}

public sealed record KanbanListReadModel
{
    [JsonPropertyName("listId")]
    public Guid ListId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; init; }

    [JsonPropertyName("cards")]
    public List<KanbanCardReadModel> Cards { get; init; } = [];
}

public sealed record KanbanCardReadModel
{
    [JsonPropertyName("cardId")]
    public Guid CardId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; init; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; init; }
}
