using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Api.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01): the
/// HealthAggregator no longer owns the state rule. Pre-HC-2 it
/// hardcoded a CriticalServices set and produced a literal
/// "HEALTHY" / "DEGRADED" / "DOWN" string from booleans only. HC-2
/// keeps the parallel <see cref="IHealthCheck"/> fan-out here (so
/// the existing /Health controller does not change shape) but
/// delegates the canonical state decision to
/// <see cref="IRuntimeStateAggregator"/>. The critical-services set
/// now lives in one place inside the concrete
/// <c>RuntimeStateAggregator</c> implementation in the host layer.
/// HealthAggregator depends only on the shared contract so layer
/// purity (api ↛ host) is preserved.
/// </summary>
public sealed class HealthAggregator
{
    private readonly IReadOnlyList<IHealthCheck> _healthChecks;
    private readonly IRuntimeStateAggregator _runtimeStateAggregator;

    public HealthAggregator(
        IEnumerable<IHealthCheck> healthChecks,
        IRuntimeStateAggregator runtimeStateAggregator)
    {
        _healthChecks = healthChecks.ToList();
        _runtimeStateAggregator = runtimeStateAggregator;
    }

    public async Task<AggregatedHealthReport> CheckAllAsync()
    {
        var tasks = _healthChecks.Select(hc => hc.CheckAsync());
        var results = await Task.WhenAll(tasks);

        // phase1.5-S5.2.4 / HC-2: canonical state decision lives in
        // RuntimeStateAggregator (host layer, behind the
        // IRuntimeStateAggregator contract). The dependency-ping
        // results are forwarded to it; the aggregator folds them
        // through the canonical rule along with breaker state, host
        // drain signal, and outbox snapshot freshness.
        var stateSnapshot = _runtimeStateAggregator.ComputeFromResults(results);

        return new AggregatedHealthReport(
            Status: MapState(stateSnapshot.State),
            Services: results,
            RuntimeState: stateSnapshot.State,
            Reasons: stateSnapshot.Reasons);
    }

    private static string MapState(RuntimeState state) => state switch
    {
        RuntimeState.Healthy => "HEALTHY",
        RuntimeState.Degraded => "DEGRADED",
        RuntimeState.NotReady => "NOT_READY",
        RuntimeState.Halt => "HALT",
        _ => "UNKNOWN",
    };
}

/// <summary>
/// phase1.5-S5.2.4 / HC-2: the report now carries the canonical
/// <see cref="RuntimeState"/> and the low-cardinality reason list
/// alongside the existing per-IHealthCheck results. The
/// <c>Status</c> string is preserved for backwards compatibility
/// with any caller reading the existing /Health body.
/// </summary>
public record AggregatedHealthReport(
    string Status,
    IReadOnlyList<HealthCheckResult> Services,
    RuntimeState RuntimeState,
    IReadOnlyList<string> Reasons
);
