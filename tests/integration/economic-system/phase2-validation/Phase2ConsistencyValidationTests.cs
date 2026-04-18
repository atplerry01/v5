using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase2Validation;

/// <summary>
/// Phase 2 validation / ledger-consistency gate.
///
/// The economic-system event store is append-only and MUST preserve three
/// invariants per aggregate:
///
///   C1 — Versions are contiguous and monotonic starting at 0.
///   C2 — Concurrent appends to the same aggregate produce contiguous,
///        non-overlapping versions (0..N-1) with zero event loss.
///   C3 — Isolation across aggregates: appending to aggregate A must not
///        mutate the version stream of aggregate B.
///
/// These are the invariants LedgerAggregate / JournalAggregate / every
/// transaction-side aggregate relies on for replay to be byte-identical.
/// Phase 2 cannot be declared complete if any of them regresses.
/// </summary>
public sealed class Phase2ConsistencyValidationTests
{
    [Fact]
    public async Task C1_Versions_Are_Contiguous_And_Monotonic()
    {
        var store = new InMemoryEventStore();
        var aggregateId = Guid.NewGuid();

        await store.AppendEventsAsync(
            aggregateId,
            new object[] { new { step = 1 }, new { step = 2 }, new { step = 3 } },
            expectedVersion: 0);

        await store.AppendEventsAsync(
            aggregateId,
            new object[] { new { step = 4 }, new { step = 5 } },
            expectedVersion: 3);

        var versions = store.Versions(aggregateId);

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, versions);
    }

    [Fact]
    public async Task C2_Concurrent_Appends_Produce_Contiguous_Versions()
    {
        var store = new InMemoryEventStore();
        var aggregateId = Guid.NewGuid();

        const int writerCount = 16;
        const int eventsPerWriter = 50;
        const int totalEvents = writerCount * eventsPerWriter;

        var gate = new ManualResetEventSlim(initialState: false);
        var writers = new Task[writerCount];

        for (var w = 0; w < writerCount; w++)
        {
            var writerId = w;
            writers[w] = Task.Run(async () =>
            {
                gate.Wait();
                for (var i = 0; i < eventsPerWriter; i++)
                {
                    await store.AppendEventsAsync(
                        aggregateId,
                        new object[] { new { writer = writerId, index = i } },
                        expectedVersion: -1);
                }
            });
        }

        gate.Set();
        await Task.WhenAll(writers);

        var versions = store.Versions(aggregateId);
        var allEvents = store.AllEvents(aggregateId);

        Assert.Equal(totalEvents, versions.Count);
        Assert.Equal(totalEvents, allEvents.Count);

        // Contiguous 0..totalEvents-1, no gaps, no duplicates.
        for (var i = 0; i < totalEvents; i++)
            Assert.Equal(i, versions[i]);
    }

    [Fact]
    public async Task C3_Cross_Aggregate_Append_Is_Isolated()
    {
        var store = new InMemoryEventStore();
        var aggA = Guid.NewGuid();
        var aggB = Guid.NewGuid();

        await store.AppendEventsAsync(aggA, new object[] { new { a = 1 } }, 0);
        await store.AppendEventsAsync(aggB, new object[] { new { b = 1 }, new { b = 2 } }, 0);
        await store.AppendEventsAsync(aggA, new object[] { new { a = 2 } }, 1);

        Assert.Equal(new[] { 0, 1 }, store.Versions(aggA));
        Assert.Equal(new[] { 0, 1 }, store.Versions(aggB));
        Assert.Equal(2, store.AllEvents(aggA).Count);
        Assert.Equal(2, store.AllEvents(aggB).Count);
    }
}
