namespace Whycespace.Shared.Contracts.Systems.Intent;

/// <summary>
/// Represents a command execution intent emitted by the systems layer.
/// Systems compose intents — runtime executes them.
/// </summary>
public sealed record ExecuteCommandIntent
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? AggregateId { get; init; }
    public string? CausationId { get; init; }
    public string? WhyceId { get; init; }
    public string? PolicyId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
