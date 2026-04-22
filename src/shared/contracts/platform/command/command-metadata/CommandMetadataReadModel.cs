namespace Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;

public sealed record CommandMetadataReadModel
{
    public Guid CommandMetadataId { get; init; }
    public Guid EnvelopeRef { get; init; }
    public string ActorId { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public string SpanId { get; init; } = string.Empty;
    public string? PolicyId { get; init; }
    public string? PolicyVersion { get; init; }
    public int TrustScore { get; init; }
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
