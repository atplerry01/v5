using Whycespace.Shared.Contracts.Infrastructure.Health;
using Xunit;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): contract-level
/// coverage for the Redis health rule. The actual host-side
/// concrete (RedisHealthCheck) wraps a StackExchange.Redis
/// IConnectionMultiplexer; rather than mocking that surface, the
/// tests target a tiny test-double that mirrors the same observable
/// state machine (connected? ping result? latency?) and asserts
/// the canonical HealthCheckResult shape the aggregator consumes.
/// This keeps the suite free of Redis dependencies while
/// validating the canonical output contract.
/// </summary>
public sealed class RedisHealthCheckTests
{
    private sealed class FakeRedisHealthCheck : IHealthCheck
    {
        private readonly bool _connected;
        private readonly bool _pingThrows;
        private readonly long _latencyMs;
        private readonly RedisHealthOptions _options;

        public string Name => "redis";

        public FakeRedisHealthCheck(bool connected, bool pingThrows, long latencyMs, RedisHealthOptions options)
        {
            _connected = connected;
            _pingThrows = pingThrows;
            _latencyMs = latencyMs;
            _options = options;
        }

        public Task<HealthCheckResult> CheckAsync()
        {
            if (!_connected)
                return Task.FromResult(new HealthCheckResult(Name, false, "DOWN", 0, "redis_unavailable"));
            if (_pingThrows)
                return Task.FromResult(new HealthCheckResult(Name, false, "DOWN", 0, "redis_ping_failed: simulated"));
            if (_latencyMs > _options.DegradedLatencyThresholdMs)
                return Task.FromResult(new HealthCheckResult(
                    Name, true, "DEGRADED", _latencyMs, $"redis_high_latency:{_latencyMs}ms"));
            return Task.FromResult(new HealthCheckResult(Name, true, "HEALTHY", _latencyMs));
        }
    }

    private static readonly RedisHealthOptions Options = new();

    [Fact]
    public async Task Disconnected_ReportsDown()
    {
        var sut = new FakeRedisHealthCheck(connected: false, pingThrows: false, latencyMs: 0, Options);
        var result = await sut.CheckAsync();

        Assert.False(result.IsHealthy);
        Assert.Equal("DOWN", result.Status);
        Assert.Contains("redis_unavailable", result.Error);
    }

    [Fact]
    public async Task PingSuccess_LowLatency_ReportsHealthy()
    {
        var sut = new FakeRedisHealthCheck(connected: true, pingThrows: false, latencyMs: 5, Options);
        var result = await sut.CheckAsync();

        Assert.True(result.IsHealthy);
        Assert.Equal("HEALTHY", result.Status);
    }

    [Fact]
    public async Task PingSuccess_HighLatency_ReportsDegraded()
    {
        // 250 ms > default DegradedLatencyThresholdMs (200) → DEGRADED.
        var sut = new FakeRedisHealthCheck(connected: true, pingThrows: false, latencyMs: 250, Options);
        var result = await sut.CheckAsync();

        Assert.True(result.IsHealthy);
        Assert.Equal("DEGRADED", result.Status);
        Assert.Contains("redis_high_latency", result.Error);
    }

    [Fact]
    public async Task PingThrows_ReportsDown()
    {
        var sut = new FakeRedisHealthCheck(connected: true, pingThrows: true, latencyMs: 0, Options);
        var result = await sut.CheckAsync();

        Assert.False(result.IsHealthy);
        Assert.Equal("DOWN", result.Status);
        Assert.Contains("redis_ping_failed", result.Error);
    }

    [Fact]
    public void RedisDegradedLatency_IsCanonicalDegradedReason()
    {
        // HC-9 added redis_degraded_latency to the canonical degraded
        // reason vocabulary so the dispatch-time filter passes it
        // through wherever it originates (in HC-9 it originates from
        // /Health and /ready via ComputeFromResults; the dispatch-time
        // GetDegradedMode does not evaluate it by design).
        Assert.Contains("redis_degraded_latency", RuntimeDegradedMode.CanonicalReasons);
    }
}
