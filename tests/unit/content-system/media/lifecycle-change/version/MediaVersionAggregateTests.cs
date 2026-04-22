using Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.LifecycleChange.Version;

public sealed class MediaVersionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static MediaVersionId NewId(string seed) =>
        new(IdGen.Generate($"MediaVersionAggregateTests:{seed}:version"));

    [Fact]
    public void Create_RaisesMediaVersionCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaVersionAggregateTests:asset-ref"));
        var fileRef = new MediaFileRef(IdGen.Generate("MediaVersionAggregateTests:file-ref"));
        var versionNumber = new MediaVersionNumber(1, 0);

        var aggregate = MediaVersionAggregate.Create(id, assetRef, versionNumber, fileRef, null, BaseTime);

        var evt = Assert.IsType<MediaVersionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.VersionId);
        Assert.Equal(assetRef, evt.AssetRef);
        Assert.Equal(versionNumber, evt.VersionNumber);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaVersionAggregateTests:asset-state"));
        var fileRef = new MediaFileRef(IdGen.Generate("MediaVersionAggregateTests:file-state"));

        var aggregate = MediaVersionAggregate.Create(id, assetRef, new MediaVersionNumber(1, 0), fileRef, null, BaseTime);

        Assert.Equal(id, aggregate.VersionId);
    }

    [Fact]
    public void Create_WithPreviousVersion_SetsLink()
    {
        var id = NewId("Create_WithPrev");
        var prevId = NewId("Prev");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaVersionAggregateTests:asset-prev"));
        var fileRef = new MediaFileRef(IdGen.Generate("MediaVersionAggregateTests:file-prev"));

        var aggregate = MediaVersionAggregate.Create(id, assetRef, new MediaVersionNumber(2, 0), fileRef, prevId, BaseTime);

        var evt = Assert.IsType<MediaVersionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(prevId, evt.PreviousVersionId);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaVersionAggregateTests:stable-asset"));
        var fileRef = new MediaFileRef(IdGen.Generate("MediaVersionAggregateTests:stable-file"));
        var v1 = MediaVersionAggregate.Create(id, assetRef, new MediaVersionNumber(1, 0), fileRef, null, BaseTime);
        var v2 = MediaVersionAggregate.Create(id, assetRef, new MediaVersionNumber(1, 0), fileRef, null, BaseTime);

        Assert.Equal(
            ((MediaVersionCreatedEvent)v1.DomainEvents[0]).VersionId.Value,
            ((MediaVersionCreatedEvent)v2.DomainEvents[0]).VersionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesMediaVersionState()
    {
        var id = NewId("History");
        var assetRef = new MediaAssetRef(IdGen.Generate("MediaVersionAggregateTests:history-asset"));
        var fileRef = new MediaFileRef(IdGen.Generate("MediaVersionAggregateTests:history-file"));

        var history = new object[]
        {
            new MediaVersionCreatedEvent(id, assetRef, new MediaVersionNumber(1, 0), fileRef, null, BaseTime)
        };

        var aggregate = (MediaVersionAggregate)Activator.CreateInstance(typeof(MediaVersionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.VersionId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
