using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.StreamCore.Availability;

public sealed class PlaybackAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureTime = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    private static PlaybackId NewId(string seed) =>
        new(IdGen.Generate($"PlaybackAggregateTests:{seed}:playback"));

    private static PlaybackSourceRef DefaultSourceRef() =>
        new(IdGen.Generate("PlaybackAggregateTests:source-ref"), PlaybackSourceKind.Stream);

    private static PlaybackWindow DefaultWindow() => new(BaseTime, FutureTime);

    [Fact]
    public void Create_RaisesPlaybackCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var sourceRef = DefaultSourceRef();
        var window = DefaultWindow();

        var aggregate = PlaybackAggregate.Create(id, sourceRef, PlaybackMode.OnDemand, window, BaseTime);

        var evt = Assert.IsType<PlaybackCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.PlaybackId);
        Assert.Equal(sourceRef, evt.SourceRef);
        Assert.Equal(PlaybackMode.OnDemand, evt.Mode);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var id = NewId("Create_State");
        var aggregate = PlaybackAggregate.Create(id, DefaultSourceRef(), PlaybackMode.OnDemand, DefaultWindow(), BaseTime);

        Assert.Equal(id, aggregate.PlaybackId);
        Assert.Equal(PlaybackStatus.Created, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var sourceRef = DefaultSourceRef();
        var window = DefaultWindow();
        var p1 = PlaybackAggregate.Create(id, sourceRef, PlaybackMode.OnDemand, window, BaseTime);
        var p2 = PlaybackAggregate.Create(id, sourceRef, PlaybackMode.OnDemand, window, BaseTime);

        Assert.Equal(
            ((PlaybackCreatedEvent)p1.DomainEvents[0]).PlaybackId.Value,
            ((PlaybackCreatedEvent)p2.DomainEvents[0]).PlaybackId.Value);
    }

    [Fact]
    public void Enable_RaisesPlaybackEnabledEvent()
    {
        var id = NewId("Enable_Valid");
        var aggregate = PlaybackAggregate.Create(id, DefaultSourceRef(), PlaybackMode.OnDemand, DefaultWindow(), BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Enable(BaseTime);

        Assert.IsType<PlaybackEnabledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(PlaybackStatus.Enabled, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesPlaybackState()
    {
        var id = NewId("History");
        var sourceRef = DefaultSourceRef();
        var window = DefaultWindow();

        var history = new object[]
        {
            new PlaybackCreatedEvent(id, sourceRef, PlaybackMode.OnDemand, window, BaseTime)
        };

        var aggregate = (PlaybackAggregate)Activator.CreateInstance(typeof(PlaybackAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.PlaybackId);
        Assert.Equal(PlaybackStatus.Created, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
