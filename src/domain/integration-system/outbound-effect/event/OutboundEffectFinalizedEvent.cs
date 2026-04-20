using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — business finality resolved. <see cref="FinalityOutcome"/> is one of
/// <c>Succeeded</c>, <c>BusinessFailed</c>, <c>PartiallyCompleted</c>,
/// <c>ManualIntervention</c>. <see cref="FinalitySource"/> distinguishes
/// Push (webhook) / Poll / SynchronousAck — used by operator tooling to
/// reconstruct the finality path.
/// </summary>
public sealed record OutboundEffectFinalizedEvent(
    AggregateId AggregateId,
    string FinalityOutcome,
    string FinalityEvidenceDigest,
    DateTimeOffset FinalizedAt,
    string FinalitySource) : DomainEvent;
