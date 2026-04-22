using Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Media.CoreObject.Asset;

public sealed class AssetAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static AssetId NewId(string seed) =>
        new(IdGen.Generate($"AssetAggregateTests:{seed}:asset"));

    [Fact]
    public void Create_RaisesAssetCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var title = new AssetTitle("Test Video");

        var aggregate = AssetAggregate.Create(id, title, AssetClassification.Video, BaseTime);

        var evt = Assert.IsType<AssetCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AssetId);
        Assert.Equal(title, evt.Title);
        Assert.Equal(AssetClassification.Video, evt.Classification);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = AssetAggregate.Create(id, new AssetTitle("My Asset"), AssetClassification.Audio, BaseTime);

        Assert.Equal(id, aggregate.AssetId);
        Assert.Equal(AssetStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var a1 = AssetAggregate.Create(id, new AssetTitle("X"), AssetClassification.Image, BaseTime);
        var a2 = AssetAggregate.Create(id, new AssetTitle("X"), AssetClassification.Image, BaseTime);

        Assert.Equal(
            ((AssetCreatedEvent)a1.DomainEvents[0]).AssetId.Value,
            ((AssetCreatedEvent)a2.DomainEvents[0]).AssetId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesAssetState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new AssetCreatedEvent(id, new AssetTitle("Historic Asset"), AssetClassification.Video, BaseTime)
        };

        var aggregate = (AssetAggregate)Activator.CreateInstance(typeof(AssetAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.AssetId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
