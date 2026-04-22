namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;

public sealed record IngestSessionReadModel
{
    public Guid SessionId { get; init; }
    public Guid BroadcastId { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset AuthenticatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
