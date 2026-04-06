namespace Whycespace.Runtime.Command;

public sealed record CommandEnvelope
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required CommandMetadata Metadata { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? AggregateId { get; init; }
}

public sealed record CommandMetadata
{
    public string? CausationId { get; init; }
    public string? WhyceId { get; init; }
    public string? PolicyId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
