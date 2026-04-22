using Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.CoreObject.Transcript;

public sealed class TranscriptAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static TranscriptId NewId(string seed) =>
        new(IdGen.Generate($"TranscriptAggregateTests:{seed}:transcript"));

    [Fact]
    public void Create_RaisesTranscriptCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var assetRef = new MediaAssetRef(IdGen.Generate("TranscriptAggregateTests:asset-ref"));

        var aggregate = TranscriptAggregate.Create(
            id, assetRef, null, TranscriptFormat.PlainText, new TranscriptLanguage("en"), BaseTime);

        var evt = Assert.IsType<TranscriptCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TranscriptId);
        Assert.Equal(assetRef, evt.AssetRef);
        Assert.Equal(TranscriptFormat.PlainText, evt.Format);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var assetRef = new MediaAssetRef(IdGen.Generate("TranscriptAggregateTests:asset-state"));

        var aggregate = TranscriptAggregate.Create(
            id, assetRef, null, TranscriptFormat.Json, new TranscriptLanguage("es"), BaseTime);

        Assert.Equal(id, aggregate.TranscriptId);
        Assert.Equal(assetRef, aggregate.AssetRef);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var assetRef = new MediaAssetRef(IdGen.Generate("TranscriptAggregateTests:stable-asset"));
        var t1 = TranscriptAggregate.Create(id, assetRef, null, TranscriptFormat.PlainText, new TranscriptLanguage("en"), BaseTime);
        var t2 = TranscriptAggregate.Create(id, assetRef, null, TranscriptFormat.PlainText, new TranscriptLanguage("en"), BaseTime);

        Assert.Equal(
            ((TranscriptCreatedEvent)t1.DomainEvents[0]).TranscriptId.Value,
            ((TranscriptCreatedEvent)t2.DomainEvents[0]).TranscriptId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesTranscriptState()
    {
        var id = NewId("History");
        var assetRef = new MediaAssetRef(IdGen.Generate("TranscriptAggregateTests:history-asset"));

        var history = new object[]
        {
            new TranscriptCreatedEvent(id, assetRef, null, TranscriptFormat.PlainText, new TranscriptLanguage("en"), BaseTime)
        };

        var aggregate = (TranscriptAggregate)Activator.CreateInstance(typeof(TranscriptAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.TranscriptId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
