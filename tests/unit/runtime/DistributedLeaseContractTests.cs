using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.C.1 — contract-level tests for <see cref="IDistributedLeaseProvider"/>.
/// Exercises acquire / release / contention / re-acquire-after-release
/// semantics against an in-memory fake that mirrors the Postgres
/// advisory-lock contract (session-scoped exclusive ownership, no TTL,
/// deterministic release on dispose).
///
/// A separate integration test (R2.A.C.2 or R5 certification) will run
/// these same scenarios against <see cref="PostgresAdvisoryLeaseProvider"/>
/// with a live Postgres instance to prove the crash-safe recovery
/// invariant holds at the database level.
/// </summary>
public sealed class DistributedLeaseContractTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
        public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
    }

    // In-memory fake that mirrors Postgres advisory-lock semantics:
    //   * TryAcquire: atomic TryAdd on a ConcurrentDictionary keyed by leaseKey.
    //   * DisposeAsync: TryRemove the key.
    //   * Session semantics: we don't simulate connection death — the
    //     handle's DisposeAsync IS the release event, analogous to
    //     the Postgres session closing.
    private sealed class InMemoryLeaseProvider : IDistributedLeaseProvider
    {
        private readonly ConcurrentDictionary<string, InMemoryLease> _held = new();
        private readonly IClock _clock;

        public InMemoryLeaseProvider(IClock clock) { _clock = clock; }

        public int ActiveCount => _held.Count;

        public Task<ILease?> TryAcquireAsync(
            string leaseKey,
            string holder,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(leaseKey))
                throw new ArgumentException(nameof(leaseKey));
            if (string.IsNullOrWhiteSpace(holder))
                throw new ArgumentException(nameof(holder));

            var candidate = new InMemoryLease(this, leaseKey, holder, _clock.UtcNow);
            if (_held.TryAdd(leaseKey, candidate))
                return Task.FromResult<ILease?>(candidate);

            return Task.FromResult<ILease?>(null);
        }

        private void Release(InMemoryLease lease) =>
            _held.TryRemove(KeyValuePair.Create(lease.Key, lease));

        private sealed class InMemoryLease : ILease
        {
            private readonly InMemoryLeaseProvider _parent;
            private int _disposed;

            public string Key { get; }
            public string Holder { get; }
            public DateTimeOffset AcquiredAt { get; }

            public InMemoryLease(
                InMemoryLeaseProvider parent,
                string key, string holder, DateTimeOffset acquiredAt)
            {
                _parent = parent;
                Key = key;
                Holder = holder;
                AcquiredAt = acquiredAt;
            }

            public ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return ValueTask.CompletedTask;
                _parent.Release(this);
                return ValueTask.CompletedTask;
            }
        }
    }

    private static (InMemoryLeaseProvider provider, FakeClock clock) NewProvider()
    {
        var clock = new FakeClock();
        return (new InMemoryLeaseProvider(clock), clock);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Happy path
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryAcquire_Returns_Handle_With_Key_Holder_And_Timestamp()
    {
        var (provider, clock) = NewProvider();
        clock.UtcNow = DateTimeOffset.Parse("2026-04-19T10:00:00Z");

        var lease = await provider.TryAcquireAsync("outbox-leader", "host-1");

        Assert.NotNull(lease);
        Assert.Equal("outbox-leader", lease!.Key);
        Assert.Equal("host-1", lease.Holder);
        Assert.Equal(DateTimeOffset.Parse("2026-04-19T10:00:00Z"), lease.AcquiredAt);

        await lease.DisposeAsync();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Contention — second acquirer sees null, NOT an exception.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryAcquire_On_Already_Held_Key_Returns_Null_Not_Exception()
    {
        var (provider, _) = NewProvider();

        var first = await provider.TryAcquireAsync("leader", "host-1");
        Assert.NotNull(first);

        var second = await provider.TryAcquireAsync("leader", "host-2");
        Assert.Null(second); // R-LEASE-CONTRACT-01: busy => null, not exception

        await first!.DisposeAsync();
    }

    [Fact]
    public async Task Different_Keys_Do_Not_Contend()
    {
        var (provider, _) = NewProvider();

        var a = await provider.TryAcquireAsync("leader-a", "host-1");
        var b = await provider.TryAcquireAsync("leader-b", "host-1");

        Assert.NotNull(a);
        Assert.NotNull(b);

        await a!.DisposeAsync();
        await b!.DisposeAsync();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Release → reacquire.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Disposing_Lease_Releases_Key_For_Reacquisition()
    {
        var (provider, _) = NewProvider();

        var first = await provider.TryAcquireAsync("leader", "host-1");
        Assert.NotNull(first);
        await first!.DisposeAsync();

        // Second holder should now succeed.
        var second = await provider.TryAcquireAsync("leader", "host-2");
        Assert.NotNull(second);
        Assert.Equal("host-2", second!.Holder);

        await second.DisposeAsync();
    }

    [Fact]
    public async Task Double_Dispose_Is_Idempotent()
    {
        var (provider, _) = NewProvider();

        var lease = await provider.TryAcquireAsync("leader", "host-1");
        Assert.NotNull(lease);

        await lease!.DisposeAsync();
        await lease.DisposeAsync(); // must not throw

        // And the key is still releasable for re-acquire after double-dispose.
        var second = await provider.TryAcquireAsync("leader", "host-2");
        Assert.NotNull(second);
        await second!.DisposeAsync();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Concurrent contention — N-way race resolves to exactly one winner.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task N_Way_Concurrent_Acquire_Has_Exactly_One_Winner()
    {
        var (provider, _) = NewProvider();
        const int racers = 50;
        using var barrier = new Barrier(racers);

        var tasks = Enumerable.Range(0, racers).Select(i => Task.Run(async () =>
        {
            barrier.SignalAndWait();
            return await provider.TryAcquireAsync("hot-key", $"host-{i}");
        })).ToArray();

        var results = await Task.WhenAll(tasks);

        var winners = results.Count(r => r is not null);
        var losers = results.Count(r => r is null);

        Assert.Equal(1, winners);
        Assert.Equal(racers - 1, losers);

        // Provider's active count reflects the single winner until released.
        Assert.Equal(1, provider.ActiveCount);

        foreach (var r in results)
            if (r is not null) await r.DisposeAsync();

        Assert.Equal(0, provider.ActiveCount);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Input validation
    // ─────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, "host-1")]
    [InlineData("", "host-1")]
    [InlineData("  ", "host-1")]
    [InlineData("leader", null)]
    [InlineData("leader", "")]
    [InlineData("leader", "  ")]
    public async Task TryAcquire_Rejects_Null_Or_Blank_Inputs(string? key, string? holder)
    {
        var (provider, _) = NewProvider();
        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.TryAcquireAsync(key!, holder!));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Audit-preservation of acquired-at timestamp
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AcquiredAt_Reflects_IClock_Reading_At_Acquire_Time()
    {
        var (provider, clock) = NewProvider();
        clock.UtcNow = DateTimeOffset.Parse("2026-04-19T09:00:00Z");

        var first = await provider.TryAcquireAsync("k", "h1");
        Assert.Equal(clock.UtcNow, first!.AcquiredAt);

        await first.DisposeAsync();

        clock.Advance(TimeSpan.FromMinutes(30));
        var second = await provider.TryAcquireAsync("k", "h2");
        Assert.Equal(clock.UtcNow, second!.AcquiredAt);

        await second.DisposeAsync();
    }
}
