namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;

public sealed record PlaybackReadModel
{
    public Guid PlaybackId { get; init; }
    public Guid SourceId { get; init; }
    public string SourceKind { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public DateTimeOffset AvailableFrom { get; init; }
    public DateTimeOffset AvailableUntil { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
