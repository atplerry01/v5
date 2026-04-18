using Whycespace.Platform.Host.Adapters;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.Platform.Host.Adapters;

/// <summary>
/// Phase 2 — T2.1 coverage for <see cref="InMemoryEnforcementDecisionCache"/>
/// restriction surface. The cache is populated synchronously by
/// <c>ApplyRestrictionHandler</c> and consulted by
/// <c>ExecutionGuardMiddleware</c> before the projection query; it is the
/// only layer that closes the projection-lag window for restrictions.
/// </summary>
public sealed class InMemoryEnforcementDecisionCacheTests
{
    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public void RecordRestriction_ThenTryGet_ReturnsSameState()
    {
        var clock = new FixedClock();
        var cache = new InMemoryEnforcementDecisionCache(clock);
        var subjectId = Guid.NewGuid();
        var state = new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: "fraud");

        cache.RecordRestriction(subjectId, state);
        var result = cache.TryGetRestriction(subjectId);

        Assert.NotNull(result);
        Assert.True(result!.IsRestricted);
        Assert.Equal("Capital", result.Scope);
        Assert.Equal("fraud", result.Reason);
    }

    [Fact]
    public void TryGetRestriction_NotRecorded_ReturnsNull()
    {
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        Assert.Null(cache.TryGetRestriction(Guid.NewGuid()));
    }

    [Fact]
    public void ClearRestriction_RemovesCachedEntry()
    {
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var subjectId = Guid.NewGuid();
        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: null));

        cache.ClearRestriction(subjectId);

        Assert.Null(cache.TryGetRestriction(subjectId));
    }

    [Fact]
    public void TryGetRestriction_ExpiredEntry_IsEvictedAndReturnsNull()
    {
        var clock = new FixedClock();
        var cache = new InMemoryEnforcementDecisionCache(clock, TimeSpan.FromSeconds(30));
        var subjectId = Guid.NewGuid();

        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: null));

        // advance past TTL
        clock.UtcNow = clock.UtcNow.AddSeconds(31);

        Assert.Null(cache.TryGetRestriction(subjectId));
    }

    [Fact]
    public void RecordRestriction_SecondRecordReplacesFirst()
    {
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var subjectId = Guid.NewGuid();

        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "Capital", Reason: "v1"));
        cache.RecordRestriction(subjectId,
            new ActiveRestrictionState(IsRestricted: true, Scope: "TRANSACTION", Reason: "v2"));

        var result = cache.TryGetRestriction(subjectId);
        Assert.Equal("TRANSACTION", result!.Scope);
        Assert.Equal("v2", result.Reason);
    }

    [Fact]
    public async Task Cache_IsThreadSafeUnderConcurrentWriters()
    {
        var cache = new InMemoryEnforcementDecisionCache(new FixedClock());
        var subjectId = Guid.NewGuid();

        var tasks = Enumerable.Range(0, 64).Select(i => Task.Run(() =>
            cache.RecordRestriction(subjectId,
                new ActiveRestrictionState(IsRestricted: true, Scope: "S", Reason: $"r{i}")))).ToArray();

        await Task.WhenAll(tasks);

        var result = cache.TryGetRestriction(subjectId);
        Assert.NotNull(result);
        Assert.True(result!.IsRestricted);
    }
}
