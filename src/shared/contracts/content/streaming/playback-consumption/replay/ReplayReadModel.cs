namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayReadModel
{
    public Guid ReplayId { get; init; }
    public Guid ArchiveId { get; init; }
    public Guid ViewerId { get; init; }
    public long PositionMs { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
