namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// Payload anchored to a WhyceChain block for identity-critical events.
/// Contains cryptographic hashes of the event data, policy decision, and execution context.
/// All fields are immutable and must be deterministically reproducible on replay.
/// </summary>
public sealed record ChainPayload
{
    public required string EventId { get; init; }
    public required string AggregateId { get; init; }
    public required string EventType { get; init; }
    public required string EventDataHash { get; init; }
    public string? PolicyDecisionHash { get; init; }
    public string? ExecutionHash { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// Optional correlation ID for idempotency (E4.1).
    /// Propagated to ChainBlock.CorrelationId for duplicate detection.
    /// </summary>
    public string? CorrelationId { get; init; }
}
