using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.StreamCore.Manifest;

public sealed class ManifestAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ManifestId NewId(string seed) =>
        new(IdGen.Generate($"ManifestAggregateTests:{seed}:manifest"));

    private static ManifestSourceRef DefaultSourceRef() =>
        new(IdGen.Generate("ManifestAggregateTests:source-ref"), ManifestSourceKind.Stream);

    [Fact]
    public void Create_RaisesManifestCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var sourceRef = DefaultSourceRef();

        var aggregate = ManifestAggregate.Create(id, sourceRef, BaseTime);

        var evt = Assert.IsType<ManifestCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ManifestId);
        Assert.Equal(sourceRef, evt.SourceRef);
        Assert.Equal(new ManifestVersion(1), evt.InitialVersion);
    }

    [Fact]
    public void Create_SetsStatusToCreated()
    {
        var id = NewId("Create_State");
        var aggregate = ManifestAggregate.Create(id, DefaultSourceRef(), BaseTime);

        Assert.Equal(id, aggregate.ManifestId);
        Assert.Equal(ManifestStatus.Created, aggregate.Status);
        Assert.Equal(new ManifestVersion(1), aggregate.CurrentVersion);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var sourceRef = DefaultSourceRef();
        var m1 = ManifestAggregate.Create(id, sourceRef, BaseTime);
        var m2 = ManifestAggregate.Create(id, sourceRef, BaseTime);

        Assert.Equal(
            ((ManifestCreatedEvent)m1.DomainEvents[0]).ManifestId.Value,
            ((ManifestCreatedEvent)m2.DomainEvents[0]).ManifestId.Value);
    }

    [Fact]
    public void Publish_RaisesManifestPublishedEvent()
    {
        var id = NewId("Publish_Valid");
        var aggregate = ManifestAggregate.Create(id, DefaultSourceRef(), BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Publish(BaseTime);

        Assert.IsType<ManifestPublishedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ManifestStatus.Published, aggregate.Status);
    }

    [Fact]
    public void LoadFromHistory_RehydratesManifestState()
    {
        var id = NewId("History");
        var sourceRef = DefaultSourceRef();

        var history = new object[]
        {
            new ManifestCreatedEvent(id, sourceRef, new ManifestVersion(1), BaseTime)
        };

        var aggregate = (ManifestAggregate)Activator.CreateInstance(typeof(ManifestAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ManifestId);
        Assert.Equal(ManifestStatus.Created, aggregate.Status);
        Assert.Equal(new ManifestVersion(1), aggregate.CurrentVersion);
        Assert.Empty(aggregate.DomainEvents);
    }
}
