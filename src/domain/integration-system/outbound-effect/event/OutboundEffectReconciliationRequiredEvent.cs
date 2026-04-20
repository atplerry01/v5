using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 / ratified constraint #4 — reconciliation-required is a first-class
/// outcome (not an ad-hoc error path). Emitted deterministically on
/// AckTimeout / FinalityWindow expiry, provider disagreement, or dispatch
/// ambiguity. R3.B.4 adds the poll/webhook-driven emission path.
/// </summary>
public sealed record OutboundEffectReconciliationRequiredEvent(
    AggregateId AggregateId,
    string Cause,
    DateTimeOffset ObservedAt) : DomainEvent;
