using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.EventStore;

/// <summary>
/// Validates the append-only ordering invariants of the event store substitute,
/// which mirrors the contract enforced by PostgresEventStoreAdapter:
///
/// - versions are monotonic per aggregate
/// - versions are contiguous (no gaps)
/// - versions are unique (no duplicates)
/// - aggregates are isolated from each other
/// </summary>
public sealed class EventOrderingTest
{
    [Fact]
    public async Task Versions_Are_Monotonic_And_Contiguous_For_A_Single_Aggregate()
    {
        var store = new InMemoryEventStore();
        var aggregateId = Guid.NewGuid();

        await store.AppendEventsAsync(aggregateId, new object[] { new Evt("a"), new Evt("b") }, expectedVersion: -1);
        await store.AppendEventsAsync(aggregateId, new object[] { new Evt("c") }, expectedVersion: -1);
        await store.AppendEventsAsync(aggregateId, new object[] { new Evt("d"), new Evt("e"), new Evt("f") }, expectedVersion: -1);

        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, store.Versions(aggregateId));
    }

    [Fact]
    public async Task Aggregates_Have_Independent_Version_Streams()
    {
        var store = new InMemoryEventStore();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        await store.AppendEventsAsync(a, new object[] { new Evt("a0"), new Evt("a1") }, -1);
        await store.AppendEventsAsync(b, new object[] { new Evt("b0") }, -1);
        await store.AppendEventsAsync(a, new object[] { new Evt("a2") }, -1);
        await store.AppendEventsAsync(b, new object[] { new Evt("b1"), new Evt("b2") }, -1);

        Assert.Equal(new[] { 0, 1, 2 }, store.Versions(a));
        Assert.Equal(new[] { 0, 1, 2 }, store.Versions(b));
    }

    [Fact]
    public async Task Empty_Append_Is_A_Noop()
    {
        var store = new InMemoryEventStore();
        var aggregateId = Guid.NewGuid();

        await store.AppendEventsAsync(aggregateId, Array.Empty<object>(), -1);

        Assert.Empty(store.Versions(aggregateId));
        Assert.Empty(await store.LoadEventsAsync(aggregateId));
    }

    [Fact]
    public async Task Loaded_Events_Preserve_Append_Order()
    {
        var store = new InMemoryEventStore();
        var aggregateId = Guid.NewGuid();

        var first = new Evt("first");
        var second = new Evt("second");
        var third = new Evt("third");

        await store.AppendEventsAsync(aggregateId, new object[] { first }, -1);
        await store.AppendEventsAsync(aggregateId, new object[] { second, third }, -1);

        var loaded = await store.LoadEventsAsync(aggregateId);
        Assert.Equal(3, loaded.Count);
        Assert.Same(first, loaded[0]);
        Assert.Same(second, loaded[1]);
        Assert.Same(third, loaded[2]);
    }

    private sealed record Evt(string Tag);
}
