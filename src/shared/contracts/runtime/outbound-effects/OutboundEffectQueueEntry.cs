namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — queue-row projection of an outbound effect. Used by
/// <see cref="IOutboundEffectQueueStore"/>. The canonical source of truth
/// remains the event stream on <c>OutboundEffectAggregate</c>; this record is
/// a performance projection that lets the relay locate pending work without
/// replaying the full event store.
/// </summary>
public sealed record OutboundEffectQueueEntry
{
    public required Guid EffectId { get; init; }
    public required string ProviderId { get; init; }
    public required string EffectType { get; init; }
    public required string IdempotencyKey { get; init; }
    public required string Status { get; init; }
    public required int AttemptCount { get; init; }
    public required int MaxAttempts { get; init; }
    public required DateTimeOffset NextAttemptAt { get; init; }
    public required DateTimeOffset DispatchDeadline { get; init; }
    public DateTimeOffset? AckDeadline { get; init; }
    public DateTimeOffset? FinalityDeadline { get; init; }
    public string? LastError { get; init; }
    public string? ClaimedBy { get; init; }
    public DateTimeOffset? ClaimedAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required object Payload { get; init; }
}
