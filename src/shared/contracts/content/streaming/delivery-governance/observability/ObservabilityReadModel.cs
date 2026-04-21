namespace Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityReadModel
{
    public Guid ObservabilityId { get; init; }
    public Guid StreamId { get; init; }
    public Guid? ArchiveId { get; init; }
    public DateTimeOffset WindowStart { get; init; }
    public DateTimeOffset WindowEnd { get; init; }
    public long Viewers { get; init; }
    public long Playbacks { get; init; }
    public long Errors { get; init; }
    public long Drops { get; init; }
    public long AverageBitrateBps { get; init; }
    public long AverageLatencyMs { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CapturedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
