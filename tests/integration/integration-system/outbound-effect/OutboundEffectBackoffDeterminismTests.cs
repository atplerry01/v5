using Whycespace.Runtime.OutboundEffects;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Shared.Kernel.Domain;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.3 / R-OUT-EFF-BACKOFF-DET-01 — pins replay-determinism of the
/// <see cref="OutboundEffectRelay.ComputeBackoffMs"/> formula. Two runs with
/// identical inputs MUST produce identical millisecond values.
/// </summary>
public sealed class OutboundEffectBackoffDeterminismTests
{
    private static readonly Guid EffectId = Guid.Parse("44444444-0000-0000-0000-000000000001");

    private static OutboundEffectOptions TestOptions() => new()
    {
        ProviderId = "backoff-test",
        DispatchTimeoutMs = 5_000,
        TotalBudgetMs = 60_000,
        AckTimeoutMs = 10_000,
        FinalityWindowMs = 60_000,
        MaxAttempts = 5,
        BaseBackoffMs = 200,
        MaxBackoffMs = 30_000,
    };

    [Fact]
    public void Two_runs_with_same_inputs_produce_identical_backoff()
    {
        var options = TestOptions();
        var random1 = new SeededRandomRecorder();
        var random2 = new SeededRandomRecorder();

        var first = OutboundEffectRelay.ComputeBackoffMs(
            options, attemptNumber: 3, EffectId, providerRetryAfter: null, random1);
        var second = OutboundEffectRelay.ComputeBackoffMs(
            options, attemptNumber: 3, EffectId, providerRetryAfter: null, random2);

        Assert.Equal(first, second);
        // Both runs consulted the same deterministic seed.
        Assert.Equal(
            OutboundEffectRelay.BackoffSeed(EffectId, 3),
            random1.LastSeed);
        Assert.Equal(random1.LastSeed, random2.LastSeed);
    }

    [Fact]
    public void Seed_encodes_effect_id_and_attempt_number()
    {
        var seed1 = OutboundEffectRelay.BackoffSeed(EffectId, 1);
        var seed2 = OutboundEffectRelay.BackoffSeed(EffectId, 2);
        var otherEffect = OutboundEffectRelay.BackoffSeed(
            Guid.Parse("55555555-0000-0000-0000-000000000001"), 1);

        Assert.NotEqual(seed1, seed2);
        Assert.NotEqual(seed1, otherEffect);
        Assert.Contains(EffectId.ToString("N"), seed1);
        Assert.Contains("retry:1", seed1);
    }

    [Fact]
    public void Provider_retry_after_takes_precedence_over_exponential_backoff()
    {
        var options = TestOptions();
        var fixedRandom = new SeededRandomRecorder();

        var backoff = OutboundEffectRelay.ComputeBackoffMs(
            options, attemptNumber: 1, EffectId,
            providerRetryAfter: TimeSpan.FromSeconds(7),
            fixedRandom);

        Assert.Equal(7_000, backoff);
        // RandomProvider was never consulted when Retry-After was present.
        Assert.Null(fixedRandom.LastSeed);
    }

    [Fact]
    public void Exponential_curve_grows_and_caps_at_max_backoff()
    {
        var options = TestOptions() with { BaseBackoffMs = 100, MaxBackoffMs = 5_000 };
        var random = new SeededRandomRecorder();

        var first = OutboundEffectRelay.ComputeBackoffMs(options, 1, EffectId, null, random);
        var second = OutboundEffectRelay.ComputeBackoffMs(options, 2, EffectId, null, random);
        var huge = OutboundEffectRelay.ComputeBackoffMs(options, 20, EffectId, null, random);

        Assert.True(second >= first, $"attempt 2 backoff ({second}ms) must be ≥ attempt 1 ({first}ms)");
        Assert.True(huge <= options.MaxBackoffMs);
    }

    [Fact]
    public void Retry_after_is_also_capped_at_max_backoff()
    {
        var options = TestOptions() with { MaxBackoffMs = 10_000 };
        var random = new SeededRandomRecorder();

        var backoff = OutboundEffectRelay.ComputeBackoffMs(
            options, 1, EffectId,
            providerRetryAfter: TimeSpan.FromSeconds(60),
            random);

        Assert.Equal(10_000, backoff);
    }

    private sealed class SeededRandomRecorder : IRandomProvider
    {
        public string? LastSeed { get; private set; }
        public double NextDouble(string seed) { LastSeed = seed; return 0.5; }
        public int NextInt(string seed, int minInclusive, int maxExclusive)
        { LastSeed = seed; return minInclusive; }
        public long NextLong(string seed) { LastSeed = seed; return 0L; }
    }
}
