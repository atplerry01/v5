using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed class IngestSessionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static IngestSessionId NewId(string seed) =>
        new(IdGen.Generate($"IngestSessionAggregateTests:{seed}:session"));

    [Fact]
    public void Authenticate_RaisesIngestSessionAuthenticatedEvent()
    {
        var id = NewId("Auth_Valid");
        var broadcastRef = new BroadcastRef(IdGen.Generate("IngestSessionAggregateTests:broadcast-ref"));
        var endpoint = new IngestEndpoint("rtmp://ingest.example.com/live/abc");

        var aggregate = IngestSessionAggregate.Authenticate(id, broadcastRef, endpoint, BaseTime);

        var evt = Assert.IsType<IngestSessionAuthenticatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SessionId);
        Assert.Equal(broadcastRef, evt.BroadcastRef);
        Assert.Equal(endpoint, evt.Endpoint);
    }

    [Fact]
    public void Authenticate_SetsStateFromEvent()
    {
        var id = NewId("Auth_State");
        var broadcastRef = new BroadcastRef(IdGen.Generate("IngestSessionAggregateTests:broadcast-state"));

        var aggregate = IngestSessionAggregate.Authenticate(
            id, broadcastRef, new IngestEndpoint("rtmp://ingest.example.com/live/def"), BaseTime);

        Assert.Equal(id, aggregate.SessionId);
        Assert.Equal(IngestSessionStatus.Authenticated, aggregate.Status);
    }

    [Fact]
    public void Authenticate_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var broadcastRef = new BroadcastRef(IdGen.Generate("IngestSessionAggregateTests:stable-broadcast"));
        var endpoint = new IngestEndpoint("rtmp://stable.example.com/live/ghi");
        var s1 = IngestSessionAggregate.Authenticate(id, broadcastRef, endpoint, BaseTime);
        var s2 = IngestSessionAggregate.Authenticate(id, broadcastRef, endpoint, BaseTime);

        Assert.Equal(
            ((IngestSessionAuthenticatedEvent)s1.DomainEvents[0]).SessionId.Value,
            ((IngestSessionAuthenticatedEvent)s2.DomainEvents[0]).SessionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesIngestSessionState()
    {
        var id = NewId("History");
        var broadcastRef = new BroadcastRef(IdGen.Generate("IngestSessionAggregateTests:history-broadcast"));
        var endpoint = new IngestEndpoint("rtmp://history.example.com/live/jkl");

        var history = new object[]
        {
            new IngestSessionAuthenticatedEvent(id, broadcastRef, endpoint, BaseTime)
        };

        var aggregate = (IngestSessionAggregate)Activator.CreateInstance(typeof(IngestSessionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.SessionId);
        Assert.Equal(IngestSessionStatus.Authenticated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
