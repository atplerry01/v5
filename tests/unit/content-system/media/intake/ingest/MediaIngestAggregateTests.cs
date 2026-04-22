using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.Intake.Ingest;

public sealed class MediaIngestAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static MediaIngestId NewId(string seed) =>
        new(IdGen.Generate($"MediaIngestAggregateTests:{seed}:ingest"));

    [Fact]
    public void Request_RaisesMediaIngestRequestedEvent()
    {
        var id = NewId("Request_Valid");
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("MediaIngestAggregateTests:source-ref"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("MediaIngestAggregateTests:input-ref"));

        var aggregate = MediaIngestAggregate.Request(id, sourceRef, inputRef, BaseTime);

        var evt = Assert.IsType<MediaIngestRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.UploadId);
        Assert.Equal(sourceRef, evt.SourceRef);
        Assert.Equal(inputRef, evt.InputRef);
    }

    [Fact]
    public void Request_SetsStateFromEvent()
    {
        var id = NewId("Request_State");
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("MediaIngestAggregateTests:source-ref-state"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("MediaIngestAggregateTests:input-ref-state"));

        var aggregate = MediaIngestAggregate.Request(id, sourceRef, inputRef, BaseTime);

        Assert.Equal(id, aggregate.UploadId);
        Assert.Equal(MediaIngestStatus.Requested, aggregate.Status);
    }

    [Fact]
    public void Request_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("MediaIngestAggregateTests:stable-source"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("MediaIngestAggregateTests:stable-input"));

        var a1 = MediaIngestAggregate.Request(id, sourceRef, inputRef, BaseTime);
        var a2 = MediaIngestAggregate.Request(id, sourceRef, inputRef, BaseTime);

        Assert.Equal(
            ((MediaIngestRequestedEvent)a1.DomainEvents[0]).UploadId.Value,
            ((MediaIngestRequestedEvent)a2.DomainEvents[0]).UploadId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesMediaIngestState()
    {
        var id = NewId("History");
        var sourceRef = new MediaIngestSourceRef(IdGen.Generate("MediaIngestAggregateTests:history-source"));
        var inputRef = new MediaIngestInputRef(IdGen.Generate("MediaIngestAggregateTests:history-input"));

        var history = new object[]
        {
            new MediaIngestRequestedEvent(id, sourceRef, inputRef, BaseTime)
        };

        var aggregate = (MediaIngestAggregate)Activator.CreateInstance(typeof(MediaIngestAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.UploadId);
        Assert.Equal(MediaIngestStatus.Requested, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
