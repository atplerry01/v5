using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.StreamCore.Channel;

public sealed class ChannelAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ChannelId NewId(string seed) =>
        new(IdGen.Generate($"ChannelAggregateTests:{seed}:channel"));

    private static StreamRef DefaultStreamRef() =>
        new(IdGen.Generate("ChannelAggregateTests:stream-ref"));

    [Fact]
    public void Create_RaisesChannelCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var streamRef = DefaultStreamRef();
        var name = new ChannelName("Alpha-Channel");

        var aggregate = ChannelAggregate.Create(id, streamRef, name, ChannelMode.Continuous, BaseTime);

        var evt = Assert.IsType<ChannelCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ChannelId);
        Assert.Equal(streamRef, evt.StreamRef);
        Assert.Equal(name, evt.Name);
        Assert.Equal(ChannelMode.Continuous, evt.Mode);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var id = NewId("Create_State");
        var aggregate = ChannelAggregate.Create(id, DefaultStreamRef(), new ChannelName("beta.channel"), ChannelMode.Continuous, BaseTime);

        Assert.Equal(id, aggregate.ChannelId);
        Assert.Equal(ChannelStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = DefaultStreamRef();
        var name = new ChannelName("stable_channel");
        var c1 = ChannelAggregate.Create(id, streamRef, name, ChannelMode.Continuous, BaseTime);
        var c2 = ChannelAggregate.Create(id, streamRef, name, ChannelMode.Continuous, BaseTime);

        Assert.Equal(
            ((ChannelCreatedEvent)c1.DomainEvents[0]).ChannelId.Value,
            ((ChannelCreatedEvent)c2.DomainEvents[0]).ChannelId.Value);
    }

    [Fact]
    public void Enable_RaisesChannelEnabledEvent()
    {
        var id = NewId("Enable_Valid");
        var aggregate = ChannelAggregate.Create(id, DefaultStreamRef(), new ChannelName("enabled-ch"), ChannelMode.Continuous, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Enable(BaseTime);

        Assert.IsType<ChannelEnabledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ChannelStatus.Enabled, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesChannelState()
    {
        var id = NewId("History");
        var streamRef = DefaultStreamRef();
        var name = new ChannelName("history-ch");

        var history = new object[]
        {
            new ChannelCreatedEvent(id, streamRef, name, ChannelMode.Continuous, BaseTime)
        };

        var aggregate = (ChannelAggregate)Activator.CreateInstance(typeof(ChannelAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ChannelId);
        Assert.Equal(ChannelStatus.Created, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
