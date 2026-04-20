using System.Collections.Concurrent;
using StackExchange.Redis;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Runtime;

#pragma warning disable IDE0005 // CircuitBreakerExtensions.ExecuteAsync is consumed below
// using directive for the extension is not required (same namespace),
// but documenting the discipline: void-returning calls use the extension
// helper (R-CIRCUIT-BREAKER-VOID-EXT-01).
#pragma warning restore IDE0005

/// <summary>
/// phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
/// Redis-backed concrete <see cref="IExecutionLockProvider"/>.
/// Uses the classic <c>SET key value NX PX ttl</c> primitive for
/// atomic acquire and a Lua compare-and-delete script for
/// owner-safe release.
///
/// Owner tokens: each acquisition is stamped with a process-unique
/// monotonic token of the form
/// <c>{MachineName}:{ProcessId}:{counter}</c>. <see cref="Guid.NewGuid"/>
/// is intentionally NOT used here ($9 forbids it codebase-wide).
/// The chosen shape is unique across processes (machine+pid),
/// unique across acquisitions within a process (Interlocked
/// counter), and trivially $9-compliant (no clock, no RNG).
///
/// Owner tokens are remembered in a process-local
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> so
/// <see cref="ReleaseAsync"/> can present the correct token to the
/// release script. The dictionary entry is removed on release so
/// the map cannot grow unbounded.
/// </summary>
public sealed class RedisExecutionLockProvider : IExecutionLockProvider
{
    // Lua compare-and-delete: only delete the key if its current
    // value matches the caller's owner token. Atomic on the Redis
    // server, so a stale process whose lease has expired cannot
    // accidentally unlock a key that has since been re-acquired
    // by another owner.
    private const string ReleaseScript = @"
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end";

    private static readonly string _processBase =
        $"{Environment.MachineName}:{Environment.ProcessId}";

    private readonly IConnectionMultiplexer _redis;
    // R2.A.D.3d / R-REDIS-BREAKER-01: shared "redis" breaker. Wraps all
    // Redis round-trips so a broker outage trips the breaker and subsequent
    // calls fast-fail without hitting Redis. Fail-closed contract (return
    // false / no-op on exception) is preserved by the outer catch below.
    private readonly ICircuitBreaker _breaker;
    private readonly ConcurrentDictionary<string, string> _owners = new(StringComparer.Ordinal);
    private long _counter;

    public RedisExecutionLockProvider(IConnectionMultiplexer redis, ICircuitBreaker breaker)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(breaker);
        _redis = redis;
        _breaker = breaker;
    }

    public async Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "TTL must be positive.");
        ct.ThrowIfCancellationRequested();

        var token = NextOwnerToken();
        // phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01) + R2.A.D.3d /
        // R-REDIS-BREAKER-OPEN-FAIL-CLOSED-01: swallow Redis
        // exceptions (transport) and breaker-open and report acquire
        // failure as a deterministic false. The contract MUST NOT
        // throw on a transient Redis outage — the runtime control
        // plane translates the false return into the canonical
        // "execution_lock_unavailable" CommandResult.Failure.
        //
        // The breaker sits INSIDE the outer try/catch: Redis failures
        // thrown by StringSetAsync bubble out of the lambda, the
        // breaker counts + re-throws, the outer catch folds into
        // the existing fail-closed default. Breaker-open short-
        // circuits without a Redis round-trip.
        try
        {
            return await _breaker.ExecuteAsync<bool>(async c =>
            {
                var db = _redis.GetDatabase();
                var ok = await db.StringSetAsync(key, token, ttl, When.NotExists);
                if (ok)
                {
                    _owners[key] = token;
                }
                return ok;
            }, ct);
        }
        catch
        {
            // Covers both CircuitBreakerOpenException (breaker fast-fail)
            // and any Redis transport exception re-thrown by the breaker.
            // Pre-R2.A.D.3d callers saw the same false; no behaviour change
            // at the API edge.
            return false;
        }
    }

    public async Task ReleaseAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!_owners.TryRemove(key, out var token))
        {
            // Caller never held this key from this process — no-op
            // by contract. Owner-safe semantics; matches the Lua
            // CAS branch on the server side.
            return;
        }
        // phase1.5-S5.2.4 / HC-9 + R2.A.D.3d: same exception-swallowing
        // discipline as TryAcquireAsync. Release is best-effort from the
        // caller's perspective — if Redis is down, the lease will lapse
        // on its own when the TTL expires. We never throw out of this
        // seam. Breaker-open short-circuits; Redis exceptions counted +
        // swallowed via the outer catch.
        try
        {
            await _breaker.ExecuteAsync(async c =>
            {
                var db = _redis.GetDatabase();
                await db.ScriptEvaluateAsync(
                    ReleaseScript,
                    new RedisKey[] { key },
                    new RedisValue[] { token });
            }, CancellationToken.None);
        }
        catch
        {
            // Best-effort release; covers both breaker-open and transport.
            // The lease will expire naturally.
        }
    }

    private string NextOwnerToken() =>
        $"{_processBase}:{Interlocked.Increment(ref _counter):x}";
}
