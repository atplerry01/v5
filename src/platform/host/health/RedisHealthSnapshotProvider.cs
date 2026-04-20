using System.Diagnostics;
using StackExchange.Redis;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Runtime;
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
    // R2.A.D.3d / R-REDIS-BREAKER-01: shared "redis" breaker. Optional for
    // back-compat with tests that don't register the breaker — when null,
    // falls back to pre-R2.A.D.3d direct Redis call with exception swallow.
    private readonly ICircuitBreaker? _breaker;

    public RedisHealthSnapshotProvider(IConnectionMultiplexer redis, IClock clock)
        : this(redis, clock, breaker: null) { }

    public RedisHealthSnapshotProvider(IConnectionMultiplexer redis, IClock clock, ICircuitBreaker? breaker)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(clock);
        _redis = redis;
        _clock = clock;
        _breaker = breaker;
    }

    public async Task<RedisHealthSnapshot> GetSnapshotAsync()
    {
        var now = _clock.UtcNow;

        // Local check — doesn't hit Redis, so it stays outside the breaker.
        if (!_redis.IsConnected)
        {
            return new RedisHealthSnapshot(
                IsHealthy: false,
                IsConnected: false,
                PingLatencyMs: null,
                CheckedAtUtc: now);
        }

        // R2.A.D.3d: ping flows through the shared "redis" breaker when
        // available. Breaker-open → unhealthy snapshot (same shape as
        // transport failure) per R-REDIS-BREAKER-OPEN-FAIL-CLOSED-01.
        try
        {
            if (_breaker is null)
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

            return await _breaker.ExecuteAsync<RedisHealthSnapshot>(async _ =>
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
            }, CancellationToken.None);
        }
        catch
        {
            // No throws cross this seam. Covers pre-breaker transport
            // exceptions, breaker-Open CircuitBreakerOpenException, and
            // breaker-wrapped transport exceptions — all fold into the
            // same fail-closed default. An exception is a healthy
            // "ping failed" observation.
            return new RedisHealthSnapshot(
                IsHealthy: false,
                IsConnected: true,
                PingLatencyMs: null,
                CheckedAtUtc: now);
        }
    }
}
