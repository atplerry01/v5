using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed class ProgressAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ProgressId NewId(string seed) =>
        new(IdGen.Generate($"ProgressAggregateTests:{seed}:progress"));

    [Fact]
    public void Track_RaisesProgressTrackedEvent()
    {
        var id = NewId("Track_Valid");
        var sessionRef = new SessionRef(IdGen.Generate("ProgressAggregateTests:session-ref"));
        var position = new PlaybackPosition(45000);

        var aggregate = ProgressAggregate.Track(id, sessionRef, position, BaseTime);

        var evt = Assert.IsType<ProgressTrackedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ProgressId);
        Assert.Equal(sessionRef, evt.SessionRef);
        Assert.Equal(position, evt.Position);
    }

    [Fact]
    public void Track_SetsStateFromEvent()
    {
        var id = NewId("Track_State");
        var sessionRef = new SessionRef(IdGen.Generate("ProgressAggregateTests:session-state"));

        var aggregate = ProgressAggregate.Track(id, sessionRef, new PlaybackPosition(0), BaseTime);

        Assert.Equal(id, aggregate.ProgressId);
    }

    [Fact]
    public void Track_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var sessionRef = new SessionRef(IdGen.Generate("ProgressAggregateTests:stable-session"));
        var position = new PlaybackPosition(30000);
        var p1 = ProgressAggregate.Track(id, sessionRef, position, BaseTime);
        var p2 = ProgressAggregate.Track(id, sessionRef, position, BaseTime);

        Assert.Equal(
            ((ProgressTrackedEvent)p1.DomainEvents[0]).ProgressId.Value,
            ((ProgressTrackedEvent)p2.DomainEvents[0]).ProgressId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesProgressState()
    {
        var id = NewId("History");
        var sessionRef = new SessionRef(IdGen.Generate("ProgressAggregateTests:history-session"));

        var history = new object[]
        {
            new ProgressTrackedEvent(id, sessionRef, new PlaybackPosition(60000), BaseTime)
        };

        var aggregate = (ProgressAggregate)Activator.CreateInstance(typeof(ProgressAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ProgressId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
