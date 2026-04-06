namespace Whyce.Runtime.Contracts;

/// <summary>
/// Runtime command envelope. Wraps any command with its execution metadata
/// for processing through the runtime control plane.
/// All commands entering the runtime are wrapped in this envelope.
/// </summary>
public sealed record RuntimeCommand
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string PolicyId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
