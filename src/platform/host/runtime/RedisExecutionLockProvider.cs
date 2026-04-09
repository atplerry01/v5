using System.Collections.Concurrent;
using StackExchange.Redis;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Platform.Host.Runtime;

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
    private readonly ConcurrentDictionary<string, string> _owners = new(StringComparer.Ordinal);
    private long _counter;

    public RedisExecutionLockProvider(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis);
        _redis = redis;
    }

    public async Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "TTL must be positive.");
        ct.ThrowIfCancellationRequested();

        var token = NextOwnerToken();
        // phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): swallow Redis
        // exceptions and report acquire failure as a deterministic
        // false. The contract MUST NOT throw on a transient Redis
        // outage — the runtime control plane translates the false
        // return into the canonical "execution_lock_unavailable"
        // CommandResult.Failure. Without this guarantee a Redis
        // hiccup would surface as an uncaught 500 instead of the
        // declared refusal family.
        try
        {
            var db = _redis.GetDatabase();
            var ok = await db.StringSetAsync(key, token, ttl, When.NotExists);
            if (ok)
            {
                _owners[key] = token;
            }
            return ok;
        }
        catch
        {
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
        // phase1.5-S5.2.4 / HC-9: same exception-swallowing
        // discipline as TryAcquireAsync. Release is best-effort
        // from the caller's perspective — if Redis is down, the
        // lease will lapse on its own when the TTL expires. We
        // never throw out of this seam.
        try
        {
            var db = _redis.GetDatabase();
            await db.ScriptEvaluateAsync(
                ReleaseScript,
                new RedisKey[] { key },
                new RedisValue[] { token });
        }
        catch
        {
            // Best-effort release. The lease will expire naturally.
        }
    }

    private string NextOwnerToken() =>
        $"{_processBase}:{Interlocked.Increment(ref _counter):x}";
}
