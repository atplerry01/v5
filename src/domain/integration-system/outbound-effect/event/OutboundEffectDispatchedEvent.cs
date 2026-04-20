using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — adapter invoked; transport-level call completed. Recorded before
/// the provider's business ack arrives (see
/// <see cref="OutboundEffectAcknowledgedEvent"/>). Acknowledged never implies
/// Finalized (ratified constraint #1).
/// </summary>
public sealed record OutboundEffectDispatchedEvent(
    AggregateId AggregateId,
    int AttemptNumber,
    DateTimeOffset DispatchStartedAt,
    DateTimeOffset DispatchCompletedAt,
    string? TransportEvidenceDigest = null) : DomainEvent;
