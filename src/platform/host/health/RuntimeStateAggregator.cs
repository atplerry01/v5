using Microsoft.Extensions.Hosting;
using Whyce.Platform.Host.Adapters;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01): canonical
/// runtime-state aggregator. Single source of truth for the
/// <see cref="RuntimeState"/> rule. Read by <see cref="Whyce.Platform.Api.Health.HealthAggregator"/>
/// in HC-2 and (in later patches) by the <c>/ready</c> endpoint, the
/// degraded-mode response contract, and any §5.3.x certification
/// harness.
///
/// HC-2 is intentionally narrow. The aggregator only consults
/// signals that are honestly readable in-process today:
///
///   - host shutdown via <see cref="IHostApplicationLifetime.ApplicationStopping"/>
///   - the existing <see cref="IHealthCheck"/> fan-out (consumed via
///     the <see cref="ComputeFromResults"/> overload that the
///     <see cref="Whyce.Platform.Api.Health.HealthAggregator"/> calls
///     after its parallel fan-out)
///   - <see cref="OpaPolicyEvaluator.IsBreakerOpen"/> via the new
///     side-effect-free public getter
///   - <see cref="WhyceChainPostgresAdapter.IsBreakerOpen"/> via the
///     same shape
///   - <see cref="IOutboxDepthSnapshot.IsFresh"/> + <c>CurrentDepth</c>
///     against <see cref="OutboxOptions.SnapshotMaxAgeSeconds"/> /
///     <see cref="OutboxOptions.HighWaterMark"/>
///
/// HC-2 explicitly does NOT consult: <c>projection.lag_seconds</c>,
/// <c>postgres.pool.acquisition_failures</c>, <c>workflow.rejected</c>,
/// <c>intake.rejected</c>, or any other meter counter. Those signals
/// are only available as <see cref="System.Diagnostics.Metrics.Meter"/>
/// streams and have no current in-process state read seam. HC-5 / HC-6 /
/// HC-8 own that work; HC-2 must not fabricate fake metric ingestion
/// (no MeterListener, no Prometheus parsing, no counter scraping).
///
/// Critical-services rule: the canonical "which IHealthCheck names are
/// load-bearing" set lives here, not in <see cref="Whyce.Platform.Api.Health.HealthAggregator"/>.
/// HC-2 preserves the existing set verbatim — { "postgres", "kafka",
/// "opa" } — but moves ownership of the rule to one place.
///
/// Order matters. NotReady dominates Degraded. Reasons are
/// low-cardinality canonical identifiers from the canonical vocabulary
/// declared in <see cref="RuntimeStateSnapshot"/>.
/// </summary>
public sealed class RuntimeStateAggregator : IRuntimeStateAggregator
{
    /// <summary>
    /// Canonical critical-services set. Pre-HC-2 this lived in
    /// <see cref="Whyce.Platform.Api.Health.HealthAggregator"/>;
    /// HC-2 moves it here so the rule has a single owner.
    /// </summary>
    public static readonly IReadOnlySet<string> CriticalHealthCheckNames =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "postgres",
            "kafka",
            "opa",
        };

    /// <summary>
    /// phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): canonical name of
    /// the synthetic health check that <c>WorkersHealthCheck</c>
    /// publishes. The aggregator recognises this name in the
    /// IHealthCheck fan-out results and elevates a failure to
    /// NotReady + reason "worker_unhealthy", positioned in the rule
    /// chain after critical_healthcheck_failed and before
    /// outbox_snapshot_stale.
    /// </summary>
    private const string WorkersHealthCheckName = "workers";

    private readonly IEnumerable<IHealthCheck> _healthChecks;
    private readonly OpaPolicyEvaluator _opaEvaluator;
    private readonly WhyceChainPostgresAdapter _chainAdapter;
    private readonly IOutboxDepthSnapshot _outboxSnapshot;
    private readonly OutboxOptions _outboxOptions;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IClock _clock;

    public RuntimeStateAggregator(
        IEnumerable<IHealthCheck> healthChecks,
        OpaPolicyEvaluator opaEvaluator,
        WhyceChainPostgresAdapter chainAdapter,
        IOutboxDepthSnapshot outboxSnapshot,
        OutboxOptions outboxOptions,
        IHostApplicationLifetime lifetime,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(healthChecks);
        ArgumentNullException.ThrowIfNull(opaEvaluator);
        ArgumentNullException.ThrowIfNull(chainAdapter);
        ArgumentNullException.ThrowIfNull(outboxSnapshot);
        ArgumentNullException.ThrowIfNull(outboxOptions);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(clock);

        _healthChecks = healthChecks;
        _opaEvaluator = opaEvaluator;
        _chainAdapter = chainAdapter;
        _outboxSnapshot = outboxSnapshot;
        _outboxOptions = outboxOptions;
        _lifetime = lifetime;
        _clock = clock;
    }

    /// <summary>
    /// Stand-alone entry point. Performs the parallel
    /// <see cref="IHealthCheck"/> fan-out and then folds the results
    /// through <see cref="ComputeFromResults"/>. Used by callers that
    /// do not already have a fan-out result set in hand.
    /// </summary>
    public async Task<RuntimeStateSnapshot> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _healthChecks.Select(hc => hc.CheckAsync());
        var results = await Task.WhenAll(tasks);
        return ComputeFromResults(results);
    }

    /// <summary>
    /// Folds an already-fetched set of <see cref="HealthCheckResult"/>
    /// into the canonical state. Called by
    /// <see cref="Whyce.Platform.Api.Health.HealthAggregator"/> after
    /// it has run its existing parallel fan-out, so the fan-out is
    /// not duplicated. The aggregation rule is identical to
    /// <see cref="GetCurrentStateAsync"/>.
    /// </summary>
    public RuntimeStateSnapshot ComputeFromResults(IReadOnlyList<HealthCheckResult> results)
    {
        var reasons = new List<string>();

        // Rule 1 — host drain dominates everything else.
        if (_lifetime.ApplicationStopping.IsCancellationRequested)
        {
            reasons.Add("host_draining");
            return new RuntimeStateSnapshot(RuntimeState.NotReady, reasons);
        }

        // Rule 2 — any critical health check failure is NotReady.
        if (results.Any(r => !r.IsHealthy && CriticalHealthCheckNames.Contains(r.Name)))
        {
            reasons.Add("critical_healthcheck_failed");
            return new RuntimeStateSnapshot(RuntimeState.NotReady, reasons);
        }

        // Rule 3 (HC-5) — worker liveness failure is NotReady. The
        // synthetic "workers" check is treated separately from the
        // critical-services set above so its reason string remains
        // distinct ("worker_unhealthy") and so it sits at its own
        // documented precedence slot. Positioned after
        // critical_healthcheck_failed and before outbox_snapshot_stale.
        if (results.Any(r => !r.IsHealthy
                             && string.Equals(r.Name, WorkersHealthCheckName, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("worker_unhealthy");
            return new RuntimeStateSnapshot(RuntimeState.NotReady, reasons);
        }

        // Rule 5 — outbox snapshot stale (NotReady) is checked
        // BEFORE breaker-open (Degraded) because stale snapshot is a
        // NotReady-class fail-safe per HC-1, even though both
        // breakers are Degraded-class. The numeric rule order in
        // the prompt has stale at #5 textually but its NotReady
        // precedence elevates it above the Degraded rules per the
        // canonical "NotReady dominates Degraded" invariant.
        if (_outboxSnapshot.HasObservation
            && !_outboxSnapshot.IsFresh(_clock.UtcNow, _outboxOptions.SnapshotMaxAgeSeconds))
        {
            reasons.Add("outbox_snapshot_stale");
            return new RuntimeStateSnapshot(RuntimeState.NotReady, reasons);
        }

        // Rule 3 / 4 / 6 / 7 — degraded contributors. Multiple
        // independent contributors may be present at once; the rule
        // collects them all into the same Degraded state with
        // distinct canonical reason identifiers.
        if (_opaEvaluator.IsBreakerOpen)
        {
            reasons.Add("opa_breaker_open");
        }

        if (_chainAdapter.IsBreakerOpen)
        {
            reasons.Add("chain_anchor_breaker_open");
        }

        if (_outboxSnapshot.HasObservation
            && _outboxSnapshot.CurrentDepth >= _outboxOptions.HighWaterMark)
        {
            reasons.Add("outbox_over_high_water_mark");
        }

        if (results.Any(r => !r.IsHealthy
                             && !CriticalHealthCheckNames.Contains(r.Name)
                             && !string.Equals(r.Name, WorkersHealthCheckName, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("noncritical_healthcheck_failed");
        }

        if (reasons.Count > 0)
        {
            return new RuntimeStateSnapshot(RuntimeState.Degraded, reasons);
        }

        return new RuntimeStateSnapshot(RuntimeState.Healthy, Array.Empty<string>());
    }
}
