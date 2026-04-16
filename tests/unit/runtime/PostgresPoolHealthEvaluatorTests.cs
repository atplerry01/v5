using Whycespace.Shared.Contracts.Infrastructure.Health;
using Xunit;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): unit coverage
/// for the canonical pool-health rule. Tests run against the pure
/// evaluator only — no DI, no DB, no host. Snapshots are
/// constructed in-line so the assertions are deterministic.
/// </summary>
public sealed class PostgresPoolHealthEvaluatorTests
{
    private static PostgresPoolSnapshot Snap(
        string name = "event-store",
        int max = 32,
        int inUse = 0,
        int failures = 0,
        double avgWaitMs = 0d,
        int recentFailures = 0) =>
        new(name, max, inUse, 0, failures, avgWaitMs, DateTime.UtcNow, recentFailures, TimeSpan.FromSeconds(60));

    [Fact]
    public void NormalPool_IsHealthy()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(inUse: 4, failures: 0, avgWaitMs: 5),
        });

        Assert.Equal(PostgresPoolHealthState.Healthy, result.State);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public void PoolAt96Percent_IsNotReady_WithExhaustedReason()
    {
        // 32 * 0.95 = 30.4 → 31 in-use trips the rule (>= 0.95).
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(max: 32, inUse: 31),
        });

        Assert.Equal(PostgresPoolHealthState.NotReady, result.State);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonPoolExhausted, result.Reasons);
    }

    [Fact]
    public void AnyRecentFailure_IsNotReady_WithFailuresReason()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(recentFailures: 1),
        });

        Assert.Equal(PostgresPoolHealthState.NotReady, result.State);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonAcquisitionFailures, result.Reasons);
    }

    [Fact]
    public void CumulativeFailuresWithoutRecent_DoNotLatchNotReady()
    {
        // HC-6 PATCH: a transient failure leaves a non-zero
        // cumulative AcquisitionFailures counter but, once it
        // ages out of the window, RecentFailures returns to 0
        // and the pool MUST report Healthy again.
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(failures: 42, recentFailures: 0),
        });

        Assert.Equal(PostgresPoolHealthState.Healthy, result.State);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public void InvalidPoolConfig_IsNotReady_WithInvalidConfigReason()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(max: 0),
        });

        Assert.Equal(PostgresPoolHealthState.NotReady, result.State);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonInvalidPoolConfig, result.Reasons);
    }

    [Fact]
    public void DeterministicReasonOrder_AcrossPools()
    {
        // HC-6 PATCH: regardless of pool iteration order, the
        // emitted reason list MUST follow the canonical order:
        // exhausted → failures → high_wait.
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(name: "a", avgWaitMs: 200d),                                  // high_wait
            Snap(name: "b", recentFailures: 3),                                // failures
            Snap(name: "c", max: 32, inUse: 31),                               // exhausted
        });

        Assert.Equal(
            new[]
            {
                PostgresPoolHealthEvaluator.ReasonPoolExhausted,
                PostgresPoolHealthEvaluator.ReasonAcquisitionFailures,
                PostgresPoolHealthEvaluator.ReasonHighWait,
            },
            result.Reasons);
    }

    [Fact]
    public void HighWaitOnly_IsDegraded_WithHighWaitReason()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(avgWaitMs: 150d),
        });

        Assert.Equal(PostgresPoolHealthState.Degraded, result.State);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonHighWait, result.Reasons);
    }

    [Fact]
    public void NotReadyDominatesDegraded()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(new[]
        {
            Snap(name: "event-store", max: 32, inUse: 31),    // pool exhausted (NotReady)
            Snap(name: "chain", avgWaitMs: 200d),              // high wait (Degraded)
        });

        Assert.Equal(PostgresPoolHealthState.NotReady, result.State);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonPoolExhausted, result.Reasons);
        Assert.Contains(PostgresPoolHealthEvaluator.ReasonHighWait, result.Reasons);
    }

    [Fact]
    public void EmptySnapshotList_IsHealthy()
    {
        var result = PostgresPoolHealthEvaluator.Evaluate(Array.Empty<PostgresPoolSnapshot>());

        Assert.Equal(PostgresPoolHealthState.Healthy, result.State);
        Assert.Empty(result.Reasons);
    }
}
