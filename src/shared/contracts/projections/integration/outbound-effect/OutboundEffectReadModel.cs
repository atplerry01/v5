namespace Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — operator-facing read model for an outbound effect. Replay-safe;
/// idempotency via <see cref="LastEventId"/>. Full event source remains the
/// aggregate stream.
/// </summary>
public sealed record OutboundEffectReadModel
{
    public required Guid EffectId { get; init; }
    public required string ProviderId { get; init; }
    public required string EffectType { get; init; }
    public required string IdempotencyKey { get; init; }
    public required string Status { get; init; }
    public int AttemptCount { get; init; }
    public int MaxAttempts { get; init; }
    public string? FailureClassification { get; init; }
    public string? FailureReason { get; init; }
    public string? ProviderOperationId { get; init; }
    public string? FinalityOutcome { get; init; }
    public string? ReconciliationCause { get; init; }

    /// <summary>R3.B.4 — ack deadline stamped on Dispatched.</summary>
    public DateTimeOffset? AckDeadline { get; init; }

    /// <summary>R3.B.4 — finality deadline stamped on Acknowledged.</summary>
    public DateTimeOffset? FinalityDeadline { get; init; }

    /// <summary>R3.B.4 — how the last Finalized transition arrived (Push / Poll / SynchronousAck).</summary>
    public string? LastFinalitySource { get; init; }

    /// <summary>R3.B.4 — digest of the last finality evidence payload for audit.</summary>
    public string? LastFinalityEvidenceDigest { get; init; }

    /// <summary>R3.B.4 — actor id of the last operator-driven reconciliation (null until Reconciled).</summary>
    public string? ReconcilerActorId { get; init; }

    public DateTimeOffset? LastTransitionAt { get; init; }
    public Guid? LastEventId { get; init; }
}
