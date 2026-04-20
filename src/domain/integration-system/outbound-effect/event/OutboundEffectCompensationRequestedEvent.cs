using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 / D-R3B1-9 — record shape lands in R3.B.1 (aggregate handler +
/// schema registered) but the relay does NOT emit the event until R3.B.5.
/// Preserves aggregate shape-stability across checkpoints.
/// </summary>
public sealed record OutboundEffectCompensationRequestedEvent(
    AggregateId AggregateId,
    string TriggeringOutcome,
    string? OwnerAggregateType = null,
    Guid? OwnerAggregateId = null) : DomainEvent;
