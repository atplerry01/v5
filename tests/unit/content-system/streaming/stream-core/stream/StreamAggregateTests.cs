using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.StreamCore.Stream;

public sealed class StreamAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static StreamId NewId(string seed) =>
        new(IdGen.Generate($"StreamAggregateTests:{seed}:stream"));

    [Fact]
    public void Create_RaisesStreamCreatedEvent()
    {
        var id = NewId("Create_Valid");

        var aggregate = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, BaseTime);

        var evt = Assert.IsType<StreamCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.StreamId);
        Assert.Equal(StreamMode.Live, evt.Mode);
        Assert.Equal(StreamType.Video, evt.Type);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var aggregate = StreamAggregate.Create(NewId("Create_Status"), StreamMode.OnDemand, StreamType.Audio, BaseTime);

        Assert.Equal(StreamStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Activate_FromCreated_RaisesStreamActivatedEvent()
    {
        var aggregate = StreamAggregate.Create(NewId("Activate_Valid"), StreamMode.Live, StreamType.Video, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Activate(BaseTime);

        Assert.IsType<StreamActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(StreamStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, BaseTime);
        var s2 = StreamAggregate.Create(id, StreamMode.Live, StreamType.Video, BaseTime);

        Assert.Equal(
            ((StreamCreatedEvent)s1.DomainEvents[0]).StreamId.Value,
            ((StreamCreatedEvent)s2.DomainEvents[0]).StreamId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesStreamState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new StreamCreatedEvent(id, StreamMode.Live, StreamType.Video, BaseTime),
            new StreamActivatedEvent(id, BaseTime)
        };

        var aggregate = (StreamAggregate)Activator.CreateInstance(typeof(StreamAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.StreamId);
        Assert.Equal(StreamStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
