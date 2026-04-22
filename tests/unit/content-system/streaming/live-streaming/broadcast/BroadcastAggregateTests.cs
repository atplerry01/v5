using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed class BroadcastAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureStart = new(new DateTimeOffset(2026, 4, 22, 14, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureEnd = new(new DateTimeOffset(2026, 4, 22, 16, 0, 0, TimeSpan.Zero));

    private static BroadcastId NewId(string seed) =>
        new(IdGen.Generate($"BroadcastAggregateTests:{seed}:broadcast"));

    [Fact]
    public void Create_RaisesBroadcastCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var streamRef = new StreamRef(IdGen.Generate("BroadcastAggregateTests:stream-ref"));

        var aggregate = BroadcastAggregate.Create(id, streamRef, BaseTime);

        var evt = Assert.IsType<BroadcastCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.BroadcastId);
        Assert.Equal(streamRef, evt.StreamRef);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var aggregate = BroadcastAggregate.Create(
            NewId("Create_Status"),
            new StreamRef(IdGen.Generate("BroadcastAggregateTests:stream-status")),
            BaseTime);

        Assert.Equal(BroadcastStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Schedule_RaisesBroadcastScheduledEvent()
    {
        var aggregate = BroadcastAggregate.Create(
            NewId("Schedule_Valid"),
            new StreamRef(IdGen.Generate("BroadcastAggregateTests:stream-sched")),
            BaseTime);
        aggregate.ClearDomainEvents();

        var window = new BroadcastWindow(FutureStart, FutureEnd);
        aggregate.Schedule(window, BaseTime);

        var evt = Assert.IsType<BroadcastScheduledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(window, evt.Window);
    }

    [Fact]
    public void Start_RaisesBroadcastStartedEvent()
    {
        var aggregate = BroadcastAggregate.Create(
            NewId("Start_Valid"),
            new StreamRef(IdGen.Generate("BroadcastAggregateTests:stream-start")),
            BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Start(BaseTime);

        Assert.IsType<BroadcastStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(BroadcastStatus.Live, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = new StreamRef(IdGen.Generate("BroadcastAggregateTests:stable-stream"));
        var b1 = BroadcastAggregate.Create(id, streamRef, BaseTime);
        var b2 = BroadcastAggregate.Create(id, streamRef, BaseTime);

        Assert.Equal(
            ((BroadcastCreatedEvent)b1.DomainEvents[0]).BroadcastId.Value,
            ((BroadcastCreatedEvent)b2.DomainEvents[0]).BroadcastId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesBroadcastState()
    {
        var id = NewId("History");
        var streamRef = new StreamRef(IdGen.Generate("BroadcastAggregateTests:history-stream"));

        var history = new object[]
        {
            new BroadcastCreatedEvent(id, streamRef, BaseTime),
            new BroadcastStartedEvent(id, BaseTime)
        };

        var aggregate = (BroadcastAggregate)Activator.CreateInstance(typeof(BroadcastAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.BroadcastId);
        Assert.Equal(BroadcastStatus.Live, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
