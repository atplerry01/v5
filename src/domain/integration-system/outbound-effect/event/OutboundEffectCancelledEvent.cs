using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — pre-dispatch cancellation (caller abort, policy disallow, deadline
/// expired before dispatch). Terminal. <see cref="PreDispatch"/> is true for
/// R3.B.1 — only pre-dispatch cancel is wired through the dispatcher. Later
/// phases may add operator-driven post-dispatch cancel pathways.
/// </summary>
public sealed record OutboundEffectCancelledEvent(
    AggregateId AggregateId,
    string CancellationReason,
    bool PreDispatch) : DomainEvent;
