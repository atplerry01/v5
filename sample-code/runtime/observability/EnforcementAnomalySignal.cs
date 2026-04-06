namespace Whycespace.Runtime.Observability;

/// <summary>
/// Represents an enforcement-level anomaly signal for T3I / GovernanceAssist pipelines.
/// Signals are emitted passively — they MUST NOT block or alter execution.
/// </summary>
public sealed class EnforcementAnomalySignal
{
    public required string Type { get; init; }
    public required string CorrelationId { get; init; }
    public required string Description { get; init; }
    public string? CommandType { get; init; }
    public string? ShardId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
