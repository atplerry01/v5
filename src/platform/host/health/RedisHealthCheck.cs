using System.Diagnostics;
using StackExchange.Redis;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): canonical Redis
/// readiness check. Pre-HC-9 this opened a per-call
/// <see cref="ConnectionMultiplexer"/> and round-tripped a
/// set/get/delete tuple — heavy on the /Health hot path and
/// duplicating the host's existing singleton multiplexer.
///
/// HC-9 replaces that with a lightweight single
/// <c>db.PingAsync()</c> probe against the singleton
/// <see cref="IConnectionMultiplexer"/>. The probe outcome maps
/// to the canonical <see cref="HealthCheckResult"/> tri-state via
/// the <c>Status</c> string ("HEALTHY" / "DEGRADED" / "DOWN")
/// because the IHealthCheck contract carries only an
/// <c>IsHealthy</c> bool. <c>RuntimeStateAggregator</c> reads
/// both the bool and the status string by name to emit the
/// canonical reason identifiers
/// (<c>redis_unhealthy</c> / <c>redis_degraded_latency</c>).
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisHealthOptions _options;

    public string Name => "redis";

    public RedisHealthCheck(IConnectionMultiplexer redis, RedisHealthOptions options)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(options);
        _redis = redis;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckAsync()
    {
        var sw = Stopwatch.StartNew();

        if (!_redis.IsConnected)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, "redis_unavailable");
        }

        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            sw.Stop();

            if (sw.ElapsedMilliseconds > _options.DegradedLatencyThresholdMs)
            {
                // IsHealthy stays true so the IHealthCheck-level
                // result is admitted; the aggregator inspects the
                // status string by name to emit the canonical
                // "redis_degraded_latency" reason.
                return new HealthCheckResult(
                    Name, true, "DEGRADED", sw.ElapsedMilliseconds,
                    $"redis_high_latency:{sw.ElapsedMilliseconds}ms");
            }

            return new HealthCheckResult(Name, true, "HEALTHY", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(Name, false, "DOWN", sw.ElapsedMilliseconds, $"redis_ping_failed: {ex.Message}");
        }
    }
}
