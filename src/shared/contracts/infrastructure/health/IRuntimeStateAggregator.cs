namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01): canonical
/// runtime-state aggregator contract. Lives in shared/contracts so
/// the api layer (<c>HealthAggregator</c>) can consume it without
/// referencing the host layer where the concrete implementation
/// lives — concrete <c>RuntimeStateAggregator</c> depends on
/// host-only adapter types (<c>OpaPolicyEvaluator</c>,
/// <c>WhyceChainPostgresAdapter</c>) so the implementation must
/// stay in host, but the contract is layer-pure.
/// </summary>
public interface IRuntimeStateAggregator
{
    /// <summary>
    /// Stand-alone entry point. Performs the parallel
    /// <see cref="IHealthCheck"/> fan-out internally and returns the
    /// canonical state snapshot.
    /// </summary>
    Task<RuntimeStateSnapshot> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Folds an already-fetched set of <see cref="HealthCheckResult"/>
    /// through the canonical state rule. Used by callers that have
    /// already run the fan-out themselves so the per-check work is
    /// not duplicated.
    /// </summary>
    RuntimeStateSnapshot ComputeFromResults(IReadOnlyList<HealthCheckResult> results);

    /// <summary>
    /// phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01):
    /// dispatch-cheap evaluation of the runtime's current
    /// Degraded-class posture. Reads only in-process state
    /// (breakers, outbox snapshot, postgres pool snapshot) — does
    /// NOT run the <see cref="IHealthCheck"/> fan-out, so it is
    /// safe to call on the per-command dispatch hot path. The
    /// returned reason list is filtered to the canonical Degraded
    /// vocabulary in <see cref="RuntimeDegradedMode.CanonicalReasons"/>;
    /// NotReady-class identifiers are never returned here.
    /// </summary>
    RuntimeDegradedMode GetDegradedMode();
}
