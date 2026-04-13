namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01): immutable
/// result of one <c>RuntimeStateAggregator.GetCurrentStateAsync</c>
/// invocation. Carries the canonical <see cref="RuntimeState"/> plus a
/// low-cardinality list of reason identifiers — no prose, no per-instance
/// payloads, no correlation ids. Reasons must be drawn from a stable,
/// canonical, audit-visible vocabulary so any consumer (operator,
/// load balancer, §5.3.x certification harness) can parse them
/// deterministically.
///
/// Canonical reason identifiers as of HC-2:
///   - "host_draining"
///   - "critical_healthcheck_failed"
///   - "opa_breaker_open"
///   - "chain_anchor_breaker_open"
///   - "outbox_snapshot_stale"
///   - "outbox_over_high_water_mark"
///   - "noncritical_healthcheck_failed"
/// </summary>
public sealed record RuntimeStateSnapshot(
    RuntimeState State,
    IReadOnlyList<string> Reasons);
