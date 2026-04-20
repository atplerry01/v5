using System.Collections.Concurrent;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.C.2 / R-LEADER-ELECTION-01 — unit coverage for
/// <see cref="LeaderElection.RunAsLeaderAsync"/>:
/// <list type="number">
///   <item>Solo instance acquires and runs <c>leaderWork</c> immediately.</item>
///   <item>Losing instance waits <c>retryInterval</c> and retries.</item>
///   <item>Cancellation exits cleanly from both contention-wait and leader-work branches.</item>
///   <item>Leader exit (work returns) releases lease and loops back to re-acquire.</item>
///   <item>Leader exception releases lease, logs, and retries after interval.</item>
/// </list>
/// Uses the in-memory <see cref="IDistributedLeaseProvider"/> fake established
/// in <see cref="DistributedLeaseContractTests"/> — same semantics as Postgres
/// advisory locks (session-scoped, no TTL, dispose releases).
/// </summary>
public sealed class LeaderElectionTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
    }

    // Re-usable in-memory lease — mirrors DistributedLeaseContractTests.InMemoryLeaseProvider.
    private sealed class InMemoryLeaseProvider : IDistributedLeaseProvider
    {
        private readonly ConcurrentDictionary<string, InMemoryLease> _held = new();
        private readonly IClock _clock;
        public int ActiveCount => _held.Count;

        public InMemoryLeaseProvider(IClock clock) { _clock = clock; }

        public Task<ILease?> TryAcquireAsync(
            string leaseKey, string holder, CancellationToken cancellationToken = default)
        {
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

            public InMemoryLease(InMemoryLeaseProvider parent, string k, string h, DateTimeOffset a)
            {
                _parent = parent; Key = k; Holder = h; AcquiredAt = a;
            }

            public ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return ValueTask.CompletedTask;
                _parent.Release(this);
                return ValueTask.CompletedTask;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Solo instance runs leader-work immediately
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Solo_Instance_Acquires_And_Runs_Leader_Work()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());
        var workRan = new TaskCompletionSource<bool>();
        using var cts = new CancellationTokenSource();

        var leaderTask = LeaderElection.RunAsLeaderAsync(
            provider,
            leaseKey: "role-a",
            holder: "host-1",
            leaderWork: ct =>
            {
                workRan.SetResult(true);
                cts.Cancel(); // end the loop after one leader tick
                return Task.CompletedTask;
            },
            retryInterval: TimeSpan.FromMilliseconds(10),
            logger: null,
            cancellationToken: cts.Token);

        await Task.WhenAny(workRan.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(workRan.Task.IsCompletedSuccessfully, "leader work should have run");
        await leaderTask;
        Assert.Equal(0, provider.ActiveCount);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Contention — losing instance retries, doesn't run work
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Loser_Does_Not_Run_Leader_Work_While_Leader_Holds_Lease()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());

        // Pre-occupy the lease so the helper sees contention from the first try.
        var occupyingLease = await provider.TryAcquireAsync("role-a", "leader-host");
        Assert.NotNull(occupyingLease);

        var workRan = new TaskCompletionSource<bool>();
        using var cts = new CancellationTokenSource();

        var loserTask = LeaderElection.RunAsLeaderAsync(
            provider,
            leaseKey: "role-a",
            holder: "loser-host",
            leaderWork: ct =>
            {
                workRan.SetResult(true);
                return Task.CompletedTask;
            },
            retryInterval: TimeSpan.FromMilliseconds(50),
            logger: null,
            cancellationToken: cts.Token);

        // Let the loser try a few times, then cancel.
        await Task.Delay(250);
        cts.Cancel();
        await loserTask;

        Assert.False(workRan.Task.IsCompleted,
            "leader work must NOT run on the losing instance while another host holds the lease");

        await occupyingLease!.DisposeAsync();
    }

    [Fact]
    public async Task Loser_Acquires_After_Leader_Releases()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());

        var occupyingLease = await provider.TryAcquireAsync("role-a", "leader-host");
        Assert.NotNull(occupyingLease);

        var workRan = new TaskCompletionSource<bool>();
        using var cts = new CancellationTokenSource();

        var waiterTask = LeaderElection.RunAsLeaderAsync(
            provider,
            leaseKey: "role-a",
            holder: "waiter-host",
            leaderWork: ct =>
            {
                workRan.SetResult(true);
                cts.Cancel();
                return Task.CompletedTask;
            },
            retryInterval: TimeSpan.FromMilliseconds(20),
            logger: null,
            cancellationToken: cts.Token);

        // Simulate the leader going away.
        await Task.Delay(100);
        await occupyingLease!.DisposeAsync();

        await Task.WhenAny(workRan.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(workRan.Task.IsCompletedSuccessfully,
            "waiter should acquire and run leader work after leader releases");

        await waiterTask;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Cancellation exits cleanly
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancellation_While_Waiting_For_Lease_Exits_Cleanly()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());
        var occupyingLease = await provider.TryAcquireAsync("role-a", "leader-host");
        Assert.NotNull(occupyingLease);

        using var cts = new CancellationTokenSource();

        var waiterTask = LeaderElection.RunAsLeaderAsync(
            provider,
            leaseKey: "role-a",
            holder: "waiter-host",
            leaderWork: _ => Task.CompletedTask,
            retryInterval: TimeSpan.FromSeconds(10), // long, so we're definitely mid-wait
            logger: null,
            cancellationToken: cts.Token);

        await Task.Delay(50);
        cts.Cancel();

        await waiterTask; // should not throw

        await occupyingLease!.DisposeAsync();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Leader work exception releases lease and retries
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Exception_In_Leader_Work_Releases_Lease_And_Retries()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());
        int attempts = 0;
        using var cts = new CancellationTokenSource();

        var leaderTask = LeaderElection.RunAsLeaderAsync(
            provider,
            leaseKey: "role-a",
            holder: "host-1",
            leaderWork: ct =>
            {
                attempts++;
                if (attempts == 1)
                    throw new InvalidOperationException("simulated leader failure");
                // Second successful leader-work → cancel to end the loop.
                cts.Cancel();
                return Task.CompletedTask;
            },
            retryInterval: TimeSpan.FromMilliseconds(20),
            logger: null,
            cancellationToken: cts.Token);

        await leaderTask;
        Assert.Equal(2, attempts);
        Assert.Equal(0, provider.ActiveCount);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Input validation
    // ─────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, "host-1")]
    [InlineData("", "host-1")]
    [InlineData("role", null)]
    [InlineData("role", "")]
    public async Task Rejects_Null_Or_Blank_Identifiers(string? key, string? holder)
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());
        await Assert.ThrowsAsync<ArgumentException>(() =>
            LeaderElection.RunAsLeaderAsync(
                provider, key!, holder!,
                leaderWork: _ => Task.CompletedTask,
                retryInterval: TimeSpan.FromSeconds(1),
                logger: null,
                cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_Non_Positive_Retry_Interval()
    {
        var provider = new InMemoryLeaseProvider(new FakeClock());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            LeaderElection.RunAsLeaderAsync(
                provider, "role", "host",
                leaderWork: _ => Task.CompletedTask,
                retryInterval: TimeSpan.Zero,
                logger: null,
                cancellationToken: CancellationToken.None));
    }
}
