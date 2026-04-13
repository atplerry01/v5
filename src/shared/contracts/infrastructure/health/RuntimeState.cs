namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01): canonical
/// runtime state model. Single source of truth for "what is the
/// runtime currently saying about itself" — read by
/// <c>HealthAggregator</c> in HC-2 and (in later patches) by the
/// <c>/ready</c> endpoint, the degraded-mode response contract, and
/// any §5.3.x certification harness.
///
/// Low-cardinality, non-flags. Order of declaration is the canonical
/// dominance order: <see cref="Halt"/> dominates <see cref="NotReady"/>
/// dominates <see cref="Degraded"/> dominates <see cref="Healthy"/>.
/// <see cref="Halt"/> remains unused in HC-2 — there is no current
/// in-process unrecoverable seam to feed it — and is reserved for a
/// later patch that introduces a declared halt input.
/// </summary>
public enum RuntimeState
{
    Healthy = 0,
    Degraded = 1,
    NotReady = 2,
    Halt = 3,
}
