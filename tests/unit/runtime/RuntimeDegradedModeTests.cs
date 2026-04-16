using Whycespace.Shared.Contracts.Infrastructure.Health;
using Xunit;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01): unit
/// coverage for the canonical filter on the RuntimeDegradedMode
/// record. Tests target the pure From(...) helper so no DI, no
/// host, no DB is involved.
/// </summary>
public sealed class RuntimeDegradedModeTests
{
    [Fact]
    public void NoCandidates_ReturnsNone_NotDegraded()
    {
        var result = RuntimeDegradedMode.From(Array.Empty<string>());

        Assert.False(result.IsDegraded);
        Assert.Empty(result.Reasons);
        Assert.Same(RuntimeDegradedMode.None, result);
    }

    [Fact]
    public void DegradedSignalsPresent_IsDegradedTrue_WithReasons()
    {
        var result = RuntimeDegradedMode.From(new[]
        {
            "opa_breaker_open",
            "outbox_over_high_water_mark",
        });

        Assert.True(result.IsDegraded);
        Assert.Equal(2, result.Reasons.Count);
        Assert.Contains("opa_breaker_open", result.Reasons);
        Assert.Contains("outbox_over_high_water_mark", result.Reasons);
    }

    [Fact]
    public void NotReadyReasons_AreFilteredOut()
    {
        // Pure NotReady reasons must NEVER appear in degraded mode.
        var result = RuntimeDegradedMode.From(new[]
        {
            "host_draining",
            "critical_healthcheck_failed",
            "worker_unhealthy",
            "outbox_snapshot_stale",
            "postgres_pool_exhausted",
            "postgres_acquisition_failures",
        });

        Assert.False(result.IsDegraded);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public void MixedReasons_OnlyDegradedSurfaces()
    {
        // NotReady identifiers are dropped; degraded identifiers
        // pass through.
        var result = RuntimeDegradedMode.From(new[]
        {
            "host_draining",                  // NotReady — dropped
            "postgres_high_wait",             // Degraded — kept
            "critical_healthcheck_failed",    // NotReady — dropped
            "chain_anchor_breaker_open",      // Degraded — kept
        });

        Assert.True(result.IsDegraded);
        Assert.Equal(2, result.Reasons.Count);
        Assert.Contains("postgres_high_wait", result.Reasons);
        Assert.Contains("chain_anchor_breaker_open", result.Reasons);
        Assert.DoesNotContain("host_draining", result.Reasons);
        Assert.DoesNotContain("critical_healthcheck_failed", result.Reasons);
    }

    [Fact]
    public void DuplicateReasons_AreDeduplicated()
    {
        var result = RuntimeDegradedMode.From(new[]
        {
            "opa_breaker_open",
            "opa_breaker_open",
            "opa_breaker_open",
        });

        Assert.True(result.IsDegraded);
        Assert.Single(result.Reasons);
    }

    [Fact]
    public void CanonicalReasonSet_MatchesSpec()
    {
        // Lock the canonical degraded reason vocabulary so any
        // future widening is a deliberate, reviewed change.
        // HC-9 widened the canonical set from 5 to 6 by adding
        // redis_degraded_latency. Lock the new size so any future
        // widening remains a deliberate, reviewed change.
        Assert.Equal(6, RuntimeDegradedMode.CanonicalReasons.Count);
        Assert.Contains("postgres_high_wait", RuntimeDegradedMode.CanonicalReasons);
        Assert.Contains("opa_breaker_open", RuntimeDegradedMode.CanonicalReasons);
        Assert.Contains("chain_anchor_breaker_open", RuntimeDegradedMode.CanonicalReasons);
        Assert.Contains("outbox_over_high_water_mark", RuntimeDegradedMode.CanonicalReasons);
        Assert.Contains("noncritical_healthcheck_failed", RuntimeDegradedMode.CanonicalReasons);
        Assert.Contains("redis_degraded_latency", RuntimeDegradedMode.CanonicalReasons);
    }
}
