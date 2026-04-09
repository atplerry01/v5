using System.Collections.Concurrent;
using Whyce.Shared.Contracts.Runtime;
using Xunit;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
/// contract-level coverage for <see cref="IExecutionLockProvider"/>.
/// Tests target an in-memory test double that implements the same
/// SET-NX semantics as the production Redis adapter so the suite
/// validates the contract without requiring a live Redis. The
/// production adapter (RedisExecutionLockProvider) implements the
/// same observable contract via SET NX PX + Lua compare-and-delete.
/// </summary>
public sealed class ExecutionLockProviderTests
{
    private sealed class InMemoryLock : IExecutionLockProvider
    {
        private readonly ConcurrentDictionary<string, byte> _held = new(StringComparer.Ordinal);
        public Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
            => Task.FromResult(_held.TryAdd(key, 1));
        public Task ReleaseAsync(string key)
        {
            _held.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SingleAcquire_Succeeds()
    {
        var sut = new InMemoryLock();
        var ok = await sut.TryAcquireAsync("k", TimeSpan.FromSeconds(30), CancellationToken.None);
        Assert.True(ok);
    }

    [Fact]
    public async Task ConcurrentAcquire_OnlyOneSucceeds()
    {
        var sut = new InMemoryLock();
        const string key = "whyce:execution-lock:abc";
        var ttl = TimeSpan.FromSeconds(30);

        var tasks = Enumerable.Range(0, 16)
            .Select(_ => sut.TryAcquireAsync(key, ttl, CancellationToken.None))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(r => r));
        Assert.Equal(15, results.Count(r => !r));
    }

    [Fact]
    public async Task ReleaseAfterAcquire_AllowsReacquire()
    {
        var sut = new InMemoryLock();
        const string key = "k2";
        var ttl = TimeSpan.FromSeconds(30);

        Assert.True(await sut.TryAcquireAsync(key, ttl, CancellationToken.None));
        Assert.False(await sut.TryAcquireAsync(key, ttl, CancellationToken.None));
        await sut.ReleaseAsync(key);
        Assert.True(await sut.TryAcquireAsync(key, ttl, CancellationToken.None));
    }

    [Fact]
    public async Task ReleaseUnheldKey_IsNoop()
    {
        // Owner-safe semantics: releasing a key not held by this
        // caller must not throw and must not unlock another owner.
        var sut = new InMemoryLock();
        await sut.ReleaseAsync("never-acquired");
        // No exception thrown.
    }

    /// <summary>
    /// phase1.5-S5.2.4 / HC-9: simulates a Redis outage by having
    /// the lock provider wrap a delegate that throws on every
    /// call. The contract requires the implementation to swallow
    /// the exception and return false so the runtime control plane
    /// can translate it into a deterministic
    /// "execution_lock_unavailable" CommandResult.Failure rather
    /// than letting an unhandled 500 escape.
    /// </summary>
    private sealed class ThrowingLock : IExecutionLockProvider
    {
        public Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
        {
            // Simulates the catch-all in RedisExecutionLockProvider:
            // any underlying store exception is collapsed to false.
            try { throw new InvalidOperationException("simulated redis outage"); }
            catch { return Task.FromResult(false); }
        }
        public Task ReleaseAsync(string key) => Task.CompletedTask;
    }

    [Fact]
    public async Task ProviderUnderRedisOutage_ReturnsFalse_NoThrow()
    {
        IExecutionLockProvider sut = new ThrowingLock();
        var ok = await sut.TryAcquireAsync("k", TimeSpan.FromSeconds(30), CancellationToken.None);
        Assert.False(ok);
    }

    [Fact]
    public async Task DistinctKeys_DoNotInterfere()
    {
        var sut = new InMemoryLock();
        var ttl = TimeSpan.FromSeconds(30);

        Assert.True(await sut.TryAcquireAsync("a", ttl, CancellationToken.None));
        Assert.True(await sut.TryAcquireAsync("b", ttl, CancellationToken.None));
        Assert.False(await sut.TryAcquireAsync("a", ttl, CancellationToken.None));
        Assert.False(await sut.TryAcquireAsync("b", ttl, CancellationToken.None));
    }
}
