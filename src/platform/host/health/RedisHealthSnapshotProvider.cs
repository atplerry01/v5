using System.Diagnostics;
using StackExchange.Redis;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): in-process snapshot
/// provider for Redis. Lives alongside <see cref="RedisHealthCheck"/>
/// so the same probe shape is available to consumers that need a
/// structured snapshot rather than a canonical
/// <see cref="HealthCheckResult"/>. The /Health controller path
/// continues to consume <see cref="RedisHealthCheck"/> via the
/// existing IHealthCheck fan-out; this provider is reserved for
/// future consumers (e.g. operator-facing diagnostics endpoints,
/// the §5.3.x certification harness).
///
/// <c>IClock</c> is injected so the snapshot timestamp is sourced
/// from the canonical Whycespace clock — never <c>DateTime.UtcNow</c>.
/// </summary>
public sealed class RedisHealthSnapshotProvider
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IClock _clock;

    public RedisHealthSnapshotProvider(IConnectionMultiplexer redis, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(clock);
        _redis = redis;
        _clock = clock;
    }

    public async Task<RedisHealthSnapshot> GetSnapshotAsync()
    {
        var now = _clock.UtcNow;

        if (!_redis.IsConnected)
        {
            return new RedisHealthSnapshot(
                IsHealthy: false,
                IsConnected: false,
                PingLatencyMs: null,
                CheckedAtUtc: now);
        }

        try
        {
            var db = _redis.GetDatabase();
            var sw = Stopwatch.StartNew();
            await db.PingAsync();
            sw.Stop();

            return new RedisHealthSnapshot(
                IsHealthy: true,
                IsConnected: true,
                PingLatencyMs: sw.ElapsedMilliseconds,
                CheckedAtUtc: now);
        }
        catch
        {
            // No throws cross this seam. The contract is "return a
            // snapshot describing what we observed", and an
            // exception is a healthy "ping failed" observation.
            return new RedisHealthSnapshot(
                IsHealthy: false,
                IsConnected: true,
                PingLatencyMs: null,
                CheckedAtUtc: now);
        }
    }
}
