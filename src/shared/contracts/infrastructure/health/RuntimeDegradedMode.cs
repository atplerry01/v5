namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01): immutable
/// per-dispatch view of the runtime's degraded posture. Carries
/// the deterministic boolean and the low-cardinality canonical
/// reasons (filtered to only the Degraded-class identifiers — never
/// any NotReady identifier). Read by <c>RuntimeControlPlane</c>
/// before pipeline execution and attached to <c>CommandContext</c>
/// so middleware, observability, and the audit pipeline can react
/// without each consumer re-evaluating the rule.
///
/// HC-7 is a non-blocking awareness signal: a degraded runtime
/// continues to admit and dispatch commands; the tag exists so
/// downstream tooling and operators can correlate behavior with
/// the live posture. Enforcement (rejection / shedding) is
/// reserved for a later workstream.
/// </summary>
public sealed record RuntimeDegradedMode(
    bool IsDegraded,
    IReadOnlyList<string> Reasons)
{
    /// <summary>
    /// Canonical Degraded-class reason vocabulary. Mirrors the
    /// Degraded branch of <c>RuntimeStateAggregator</c>'s rule
    /// chain. <c>RuntimeDegradedMode.From</c> filters arbitrary
    /// reason inputs through this set so a NotReady-class identifier
    /// can never accidentally surface as a Degraded reason.
    /// </summary>
    public static readonly IReadOnlySet<string> CanonicalReasons =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "postgres_high_wait",
            "opa_breaker_open",
            "chain_anchor_breaker_open",
            "outbox_over_high_water_mark",
            "noncritical_healthcheck_failed",
            // phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): Redis high
            // ping latency surfaces as a Degraded contributor via
            // /Health and /ready. Not evaluated on the dispatch
            // hot path (would require an IHealthCheck fan-out);
            // the canonical entry exists so the filter passes it
            // through wherever it does originate.
            "redis_degraded_latency",
        };

    /// <summary>
    /// Canonical "no degradation" sentinel. Allocation-free —
    /// safe for the dispatch hot path.
    /// </summary>
    public static RuntimeDegradedMode None { get; } =
        new(false, Array.Empty<string>());

    /// <summary>
    /// Builds a <see cref="RuntimeDegradedMode"/> from a candidate
    /// reason list. Preserves declaration order, deduplicates, and
    /// drops any reason not in <see cref="CanonicalReasons"/> —
    /// guaranteeing that NotReady identifiers can never leak into
    /// the degraded surface.
    /// </summary>
    public static RuntimeDegradedMode From(IEnumerable<string> reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        var filtered = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in reasons)
        {
            if (r is null) continue;
            if (!CanonicalReasons.Contains(r)) continue;
            if (seen.Add(r)) filtered.Add(r);
        }
        return filtered.Count == 0
            ? None
            : new RuntimeDegradedMode(true, filtered);
    }
}
