using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed class ArchiveAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ArchiveId NewId(string seed) =>
        new(IdGen.Generate($"ArchiveAggregateTests:{seed}:archive"));

    [Fact]
    public void Start_RaisesArchiveStartedEvent()
    {
        var id = NewId("Start_Valid");
        var streamRef = new StreamRef(IdGen.Generate("ArchiveAggregateTests:stream-ref"));

        var aggregate = ArchiveAggregate.Start(id, streamRef, null, BaseTime);

        var evt = Assert.IsType<ArchiveStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ArchiveId);
        Assert.Equal(streamRef, evt.StreamRef);
        Assert.Null(evt.SessionRef);
    }

    [Fact]
    public void Start_WithSessionRef_SetsSessionRef()
    {
        var id = NewId("Start_WithSession");
        var streamRef = new StreamRef(IdGen.Generate("ArchiveAggregateTests:stream-session"));
        var sessionRef = new StreamSessionRef(IdGen.Generate("ArchiveAggregateTests:session-ref"));

        var aggregate = ArchiveAggregate.Start(id, streamRef, sessionRef, BaseTime);

        var evt = Assert.IsType<ArchiveStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(sessionRef, evt.SessionRef);
    }

    [Fact]
    public void Start_SetsStateFromEvent()
    {
        var id = NewId("Start_State");
        var streamRef = new StreamRef(IdGen.Generate("ArchiveAggregateTests:stream-state"));

        var aggregate = ArchiveAggregate.Start(id, streamRef, null, BaseTime);

        Assert.Equal(id, aggregate.ArchiveId);
        Assert.Equal(ArchiveStatus.Started, aggregate.Status);
    }

    [Fact]
    public void Start_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = new StreamRef(IdGen.Generate("ArchiveAggregateTests:stable-stream"));
        var a1 = ArchiveAggregate.Start(id, streamRef, null, BaseTime);
        var a2 = ArchiveAggregate.Start(id, streamRef, null, BaseTime);

        Assert.Equal(
            ((ArchiveStartedEvent)a1.DomainEvents[0]).ArchiveId.Value,
            ((ArchiveStartedEvent)a2.DomainEvents[0]).ArchiveId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesArchiveState()
    {
        var id = NewId("History");
        var streamRef = new StreamRef(IdGen.Generate("ArchiveAggregateTests:history-stream"));

        var history = new object[]
        {
            new ArchiveStartedEvent(id, streamRef, null, BaseTime)
        };

        var aggregate = (ArchiveAggregate)Activator.CreateInstance(typeof(ArchiveAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ArchiveId);
        Assert.Equal(ArchiveStatus.Started, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
