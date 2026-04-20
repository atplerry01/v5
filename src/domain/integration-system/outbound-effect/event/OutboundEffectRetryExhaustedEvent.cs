using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — retry budget exhausted. Terminal refusal; ignites compensation
/// path in R3.B.5 when a compensation workflow is registered.
/// <c>RetryExhausted</c> IS the DLQ — no separate dead-letter topic is
/// required for outbound effects.
/// </summary>
public sealed record OutboundEffectRetryExhaustedEvent(
    AggregateId AggregateId,
    int TotalAttempts,
    string FinalClassification) : DomainEvent;
