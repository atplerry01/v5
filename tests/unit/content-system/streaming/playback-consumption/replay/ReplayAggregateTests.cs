using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed class ReplayAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ReplayId NewId(string seed) =>
        new(IdGen.Generate($"ReplayAggregateTests:{seed}:replay"));

    [Fact]
    public void Request_RaisesReplayRequestedEvent()
    {
        var id = NewId("Request_Valid");
        var archiveRef = new ArchiveRef(IdGen.Generate("ReplayAggregateTests:archive-ref"));
        var viewerRef = new ViewerRef(IdGen.Generate("ReplayAggregateTests:viewer-ref"));

        var aggregate = ReplayAggregate.Request(id, archiveRef, viewerRef, BaseTime);

        var evt = Assert.IsType<ReplayRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ReplayId);
        Assert.Equal(archiveRef, evt.ArchiveRef);
        Assert.Equal(viewerRef, evt.ViewerRef);
    }

    [Fact]
    public void Request_SetsStateFromEvent()
    {
        var id = NewId("Request_State");
        var archiveRef = new ArchiveRef(IdGen.Generate("ReplayAggregateTests:archive-state"));
        var viewerRef = new ViewerRef(IdGen.Generate("ReplayAggregateTests:viewer-state"));

        var aggregate = ReplayAggregate.Request(id, archiveRef, viewerRef, BaseTime);

        Assert.Equal(id, aggregate.ReplayId);
        Assert.Equal(ReplayStatus.Requested, aggregate.Status);
    }

    [Fact]
    public void Request_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var archiveRef = new ArchiveRef(IdGen.Generate("ReplayAggregateTests:stable-archive"));
        var viewerRef = new ViewerRef(IdGen.Generate("ReplayAggregateTests:stable-viewer"));
        var r1 = ReplayAggregate.Request(id, archiveRef, viewerRef, BaseTime);
        var r2 = ReplayAggregate.Request(id, archiveRef, viewerRef, BaseTime);

        Assert.Equal(
            ((ReplayRequestedEvent)r1.DomainEvents[0]).ReplayId.Value,
            ((ReplayRequestedEvent)r2.DomainEvents[0]).ReplayId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesReplayState()
    {
        var id = NewId("History");
        var archiveRef = new ArchiveRef(IdGen.Generate("ReplayAggregateTests:history-archive"));
        var viewerRef = new ViewerRef(IdGen.Generate("ReplayAggregateTests:history-viewer"));

        var history = new object[]
        {
            new ReplayRequestedEvent(id, archiveRef, viewerRef, BaseTime)
        };

        var aggregate = (ReplayAggregate)Activator.CreateInstance(typeof(ReplayAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ReplayId);
        Assert.Equal(ReplayStatus.Requested, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
