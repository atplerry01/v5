using Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed class MediaProcessingAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static MediaProcessingJobId NewId(string seed) =>
        new(IdGen.Generate($"MediaProcessingAggregateTests:{seed}:job"));

    [Fact]
    public void Request_RaisesMediaProcessingRequestedEvent()
    {
        var id = NewId("Request_Valid");
        var inputRef = new MediaProcessingInputRef(IdGen.Generate("MediaProcessingAggregateTests:input-ref"));

        var aggregate = MediaProcessingAggregate.Request(id, MediaProcessingKind.Transcode, inputRef, BaseTime);

        var evt = Assert.IsType<MediaProcessingRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.JobId);
        Assert.Equal(MediaProcessingKind.Transcode, evt.Kind);
        Assert.Equal(inputRef, evt.InputRef);
    }

    [Fact]
    public void Request_SetsStatusToRequested()
    {
        var id = NewId("Request_State");
        var inputRef = new MediaProcessingInputRef(IdGen.Generate("MediaProcessingAggregateTests:input-state"));

        var aggregate = MediaProcessingAggregate.Request(id, MediaProcessingKind.Normalize, inputRef, BaseTime);

        Assert.Equal(id, aggregate.JobId);
        Assert.Equal(MediaProcessingStatus.Requested, aggregate.Status);
    }

    [Fact]
    public void Request_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var inputRef = new MediaProcessingInputRef(IdGen.Generate("MediaProcessingAggregateTests:stable-input"));
        var p1 = MediaProcessingAggregate.Request(id, MediaProcessingKind.Transcode, inputRef, BaseTime);
        var p2 = MediaProcessingAggregate.Request(id, MediaProcessingKind.Transcode, inputRef, BaseTime);

        Assert.Equal(
            ((MediaProcessingRequestedEvent)p1.DomainEvents[0]).JobId.Value,
            ((MediaProcessingRequestedEvent)p2.DomainEvents[0]).JobId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesMediaProcessingState()
    {
        var id = NewId("History");
        var inputRef = new MediaProcessingInputRef(IdGen.Generate("MediaProcessingAggregateTests:history-input"));

        var history = new object[]
        {
            new MediaProcessingRequestedEvent(id, MediaProcessingKind.Transcode, inputRef, BaseTime)
        };

        var aggregate = (MediaProcessingAggregate)Activator.CreateInstance(typeof(MediaProcessingAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.JobId);
        Assert.Equal(MediaProcessingStatus.Requested, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
