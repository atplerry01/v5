namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;

public sealed record ProgressReadModel
{
    public Guid ProgressId { get; init; }
    public Guid SessionId { get; init; }
    public long PositionMs { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset TrackedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
