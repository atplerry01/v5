namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;

public sealed record ArchiveReadModel
{
    public Guid ArchiveId { get; init; }
    public Guid StreamId { get; init; }
    public Guid? SessionId { get; init; }
    public Guid? OutputId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
