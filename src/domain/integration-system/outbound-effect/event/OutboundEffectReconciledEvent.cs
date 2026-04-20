using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — reconciliation resolved. <see cref="Outcome"/> is one of
/// <c>Succeeded</c>, <c>BusinessFailed</c>, <c>PartiallyCompleted</c>,
/// <c>ManualIntervention</c>. Replay-equivalent to the Finalized path for the
/// same outcome; the event distinguishes the resolution ritual.
/// </summary>
public sealed record OutboundEffectReconciledEvent(
    AggregateId AggregateId,
    string Outcome,
    string EvidenceDigest,
    string ReconcilerActorId) : DomainEvent;
