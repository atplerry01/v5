using Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.Descriptor.Metadata;

public sealed class MediaMetadataAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static MediaMetadataId NewId(string seed) =>
        new(IdGen.Generate($"MediaMetadataAggregateTests:{seed}:metadata"));

    [Fact]
    public void Create_RaisesMediaMetadataCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaMetadataAggregateTests:asset-ref"));

        var aggregate = MediaMetadataAggregate.Create(id, assetRef, BaseTime);

        var evt = Assert.IsType<MediaMetadataCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.MetadataId);
        Assert.Equal(assetRef, evt.AssetRef);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaMetadataAggregateTests:asset-state"));

        var aggregate = MediaMetadataAggregate.Create(id, assetRef, BaseTime);

        Assert.Equal(id, aggregate.MetadataId);
        Assert.Equal(assetRef, aggregate.AssetRef);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaMetadataAggregateTests:stable-asset"));
        var m1 = MediaMetadataAggregate.Create(id, assetRef, BaseTime);
        var m2 = MediaMetadataAggregate.Create(id, assetRef, BaseTime);

        Assert.Equal(
            ((MediaMetadataCreatedEvent)m1.DomainEvents[0]).MetadataId.Value,
            ((MediaMetadataCreatedEvent)m2.DomainEvents[0]).MetadataId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesMediaMetadataState()
    {
        var id = NewId("History");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaMetadataAggregateTests:history-asset"));

        var history = new object[]
        {
            new MediaMetadataCreatedEvent(id, assetRef, BaseTime)
        };

        var aggregate = (MediaMetadataAggregate)Activator.CreateInstance(typeof(MediaMetadataAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.MetadataId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
