using Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.CoreObject.Subtitle;

public sealed class SubtitleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static SubtitleId NewId(string seed) =>
        new(IdGen.Generate($"SubtitleAggregateTests:{seed}:subtitle"));

    [Fact]
    public void Create_RaisesSubtitleCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var assetRef = new MediaAssetRef(IdGen.Generate("SubtitleAggregateTests:asset-ref"));

        var aggregate = SubtitleAggregate.Create(
            id, assetRef, null, SubtitleFormat.WebVtt, new SubtitleLanguage("en"), null, BaseTime);

        var evt = Assert.IsType<SubtitleCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SubtitleId);
        Assert.Equal(assetRef, evt.AssetRef);
        Assert.Equal(SubtitleFormat.WebVtt, evt.Format);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var assetRef = new MediaAssetRef(IdGen.Generate("SubtitleAggregateTests:asset-state"));

        var aggregate = SubtitleAggregate.Create(
            id, assetRef, null, SubtitleFormat.Srt, new SubtitleLanguage("fr"), null, BaseTime);

        Assert.Equal(id, aggregate.SubtitleId);
        Assert.Equal(assetRef, aggregate.AssetRef);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var assetRef = new MediaAssetRef(IdGen.Generate("SubtitleAggregateTests:stable-asset"));
        var s1 = SubtitleAggregate.Create(id, assetRef, null, SubtitleFormat.WebVtt, new SubtitleLanguage("en"), null, BaseTime);
        var s2 = SubtitleAggregate.Create(id, assetRef, null, SubtitleFormat.WebVtt, new SubtitleLanguage("en"), null, BaseTime);

        Assert.Equal(
            ((SubtitleCreatedEvent)s1.DomainEvents[0]).SubtitleId.Value,
            ((SubtitleCreatedEvent)s2.DomainEvents[0]).SubtitleId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSubtitleState()
    {
        var id = NewId("History");
        var assetRef = new MediaAssetRef(IdGen.Generate("SubtitleAggregateTests:history-asset"));

        var history = new object[]
        {
            new SubtitleCreatedEvent(id, assetRef, null, SubtitleFormat.WebVtt, new SubtitleLanguage("en"), null, BaseTime)
        };

        var aggregate = (SubtitleAggregate)Activator.CreateInstance(typeof(SubtitleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.SubtitleId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
