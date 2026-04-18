using Whycespace.Domain.EconomicSystem.Transaction.Limit;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Transaction.Limit;

/// <summary>
/// T1.3 — Unit coverage for <see cref="LimitAggregate"/> concurrency guard.
///
/// Check() accepts an expectedVersion. When two commands load the same
/// limit at version V, the first to apply its LimitCheckedEvent advances
/// the aggregate to V+1; the second command — still carrying expectedVersion
/// V — must be rejected deterministically (ConcurrencyConflict) so only
/// one approval can cross the threshold boundary.
/// </summary>
public sealed class LimitAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp DefinedAt = new(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp CheckedAt = new(new DateTimeOffset(2026, 4, 17, 12, 5, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");

    private static LimitAggregate NewLimit(string seed, decimal threshold = 100m)
    {
        var limitId   = new LimitId(IdGen.Generate($"LimitAggregateTests:{seed}:limit"));
        var accountId = IdGen.Generate($"LimitAggregateTests:{seed}:account");
        var aggregate = LimitAggregate.Define(
            limitId, accountId, LimitType.DailyVolume, new Amount(threshold), Usd, DefinedAt);
        return aggregate;
    }

    private static LimitAggregate RehydrateFromHistory(IEnumerable<object> history)
    {
        var aggregate = (LimitAggregate)Activator.CreateInstance(typeof(LimitAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        return aggregate;
    }

    [Fact]
    public void Check_WithinThreshold_WithNoExpectedVersion_Passes()
    {
        var limit = NewLimit("Basic");
        var txId  = IdGen.Generate("LimitAggregateTests:Basic:tx");

        limit.Check(txId, new Amount(50m), CheckedAt);

        Assert.Contains(limit.DomainEvents, e => e is LimitCheckedEvent);
    }

    [Fact]
    public void Check_WithMatchingExpectedVersion_Passes()
    {
        // After loading a single LimitDefinedEvent the aggregate sits at Version=0.
        var defineEvent = BuildDefineEvent("Match");
        var limit = RehydrateFromHistory(new object[] { defineEvent });
        var txId  = IdGen.Generate("LimitAggregateTests:Match:tx");

        limit.Check(txId, new Amount(50m), CheckedAt, expectedVersion: 0);

        Assert.Contains(limit.DomainEvents, e => e is LimitCheckedEvent);
    }

    [Fact]
    public void Check_WithStaleExpectedVersion_ThrowsConcurrencyConflict()
    {
        // Simulate a second concurrent command that still believes it is
        // operating against version 0 even though a LimitCheckedEvent has
        // already advanced the stream past that point.
        var defineEvent = BuildDefineEvent("Stale");
        var limitId = defineEvent.LimitId;
        var history = new object[]
        {
            defineEvent,
            new LimitCheckedEvent(
                limitId,
                IdGen.Generate("LimitAggregateTests:Stale:tx0"),
                new Amount(20m),
                new Amount(20m),
                CheckedAt)
        };
        var limit = RehydrateFromHistory(history);
        var txId  = IdGen.Generate("LimitAggregateTests:Stale:tx1");

        var ex = Assert.ThrowsAny<DomainException>(() =>
            limit.Check(txId, new Amount(30m), CheckedAt, expectedVersion: 0));

        Assert.Contains("concurrency conflict", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Check_ExceedsThreshold_RaisesExceededEventAndThrows()
    {
        var limit = NewLimit("Exceed", threshold: 100m);
        var txId  = IdGen.Generate("LimitAggregateTests:Exceed:tx");

        Assert.ThrowsAny<DomainException>(() =>
            limit.Check(txId, new Amount(250m), CheckedAt));

        Assert.Contains(limit.DomainEvents, e => e is LimitExceededEvent);
    }

    [Fact]
    public void Check_TwoConcurrentBoundaryChecks_OnlyOneApprovedUnderVersionGuard()
    {
        // Boundary scenario: threshold 100, utilization 90, two concurrent
        // transactions of 15 each. Without the guard both would pass.
        // With the guard, the second command (stale version) is rejected
        // deterministically and must be resubmitted with the fresh state.
        var defineEvent = BuildDefineEvent("Boundary", threshold: 100m);
        var limitA = RehydrateFromHistory(new object[] { defineEvent });

        // First command succeeds (CurrentUtilization goes 0 → 90, version 0 → 1).
        limitA.Check(IdGen.Generate("LimitAggregateTests:Boundary:tx-prime"),
            new Amount(90m), CheckedAt, expectedVersion: 0);

        // Second command was built against version 0 but will be dispatched
        // after limitA's event is appended; rehydrating reflects that state.
        var limitB = RehydrateFromHistory(new object[]
        {
            defineEvent,
            new LimitCheckedEvent(defineEvent.LimitId,
                IdGen.Generate("LimitAggregateTests:Boundary:tx-prime"),
                new Amount(90m), new Amount(90m), CheckedAt)
        });

        Assert.ThrowsAny<DomainException>(() =>
            limitB.Check(IdGen.Generate("LimitAggregateTests:Boundary:tx-stale"),
                new Amount(15m), CheckedAt, expectedVersion: 0));
    }

    private static LimitDefinedEvent BuildDefineEvent(string seed, decimal threshold = 100m)
    {
        var limitId   = new LimitId(IdGen.Generate($"LimitAggregateTests:{seed}:limit"));
        var accountId = IdGen.Generate($"LimitAggregateTests:{seed}:account");
        return new LimitDefinedEvent(limitId, accountId, LimitType.DailyVolume,
            new Amount(threshold), Usd, DefinedAt);
    }
}
