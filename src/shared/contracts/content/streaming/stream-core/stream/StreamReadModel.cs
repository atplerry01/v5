namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;

public sealed record StreamReadModel
{
    public Guid StreamId { get; init; }
    public string Mode { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
