namespace Whycespace.Shared.Contracts.Platform.Event.EventMetadata;

public sealed record EventMetadataReadModel
{
    public Guid EventMetadataId { get; init; }
    public Guid EnvelopeRef { get; init; }
    public string ExecutionHash { get; init; } = string.Empty;
    public string PolicyDecisionHash { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public string SpanId { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
