using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed class SessionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureTime = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    private static SessionId NewId(string seed) =>
        new(IdGen.Generate($"SessionAggregateTests:{seed}:session"));

    [Fact]
    public void Open_RaisesSessionOpenedEvent()
    {
        var id = NewId("Open_Valid");
        var streamRef = new StreamRef(IdGen.Generate("SessionAggregateTests:stream-ref"));
        var window = new SessionWindow(BaseTime, FutureTime);

        var aggregate = SessionAggregate.Open(id, streamRef, window, BaseTime);

        var evt = Assert.IsType<SessionOpenedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SessionId);
        Assert.Equal(streamRef, evt.StreamRef);
    }

    [Fact]
    public void Open_SetsStatusToOpen()
    {
        var id = NewId("Open_State");
        var streamRef = new StreamRef(IdGen.Generate("SessionAggregateTests:stream-state"));
        var window = new SessionWindow(BaseTime, FutureTime);

        var aggregate = SessionAggregate.Open(id, streamRef, window, BaseTime);

        Assert.Equal(id, aggregate.SessionId);
        Assert.Equal(SessionStatus.Opened, aggregate.Status);
    }

    [Fact]
    public void Open_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = new StreamRef(IdGen.Generate("SessionAggregateTests:stable-stream"));
        var window = new SessionWindow(BaseTime, FutureTime);
        var s1 = SessionAggregate.Open(id, streamRef, window, BaseTime);
        var s2 = SessionAggregate.Open(id, streamRef, window, BaseTime);

        Assert.Equal(
            ((SessionOpenedEvent)s1.DomainEvents[0]).SessionId.Value,
            ((SessionOpenedEvent)s2.DomainEvents[0]).SessionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSessionState()
    {
        var id = NewId("History");
        var streamRef = new StreamRef(IdGen.Generate("SessionAggregateTests:history-stream"));
        var window = new SessionWindow(BaseTime, FutureTime);

        var history = new object[]
        {
            new SessionOpenedEvent(id, streamRef, window, BaseTime)
        };

        var aggregate = (SessionAggregate)Activator.CreateInstance(typeof(SessionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.SessionId);
        Assert.Equal(SessionStatus.Opened, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
