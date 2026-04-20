using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — adapter returned <c>DispatchFailedPreAcceptance</c> or similar
/// transport-level failure. <see cref="Classification"/> is the canonical
/// three-way discriminator (<c>Transient</c> | <c>Terminal</c> |
/// <c>Ambiguous</c>) that drives retry vs terminal vs reconciliation.
/// </summary>
public sealed record OutboundEffectDispatchFailedEvent(
    AggregateId AggregateId,
    int AttemptNumber,
    string Classification,
    string Reason,
    int? RetryAfterMs = null) : DomainEvent;
