using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 / D-R3B-2 — one event per retry attempt (approved default — batched
/// attempt-summary is the alternative considered and rejected). Preserves
/// symmetry with R3.A retry evidence under §14 audit row.
/// </summary>
public sealed record OutboundEffectRetryAttemptedEvent(
    AggregateId AggregateId,
    int AttemptNumber,
    DateTimeOffset NextAttemptAt,
    int BackoffMs,
    string PrecedingClassification) : DomainEvent;
