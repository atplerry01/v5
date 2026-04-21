namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastReadModel
{
    public Guid BroadcastId { get; init; }
    public Guid StreamId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? ScheduledStart { get; init; }
    public DateTimeOffset? ScheduledEnd { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public string? CancellationReason { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
