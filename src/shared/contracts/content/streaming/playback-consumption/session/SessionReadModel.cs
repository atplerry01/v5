namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;

public sealed record SessionReadModel
{
    public Guid SessionId { get; init; }
    public Guid StreamId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TerminationReason { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
