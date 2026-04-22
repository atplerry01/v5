using Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.CoreObject.Bundle;

public sealed class DocumentBundleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentBundleId NewId(string seed) =>
        new(IdGen.Generate($"DocumentBundleAggregateTests:{seed}:bundle"));

    [Fact]
    public void Create_RaisesDocumentBundleCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var name = new BundleName("Annual Reports Bundle");

        var aggregate = DocumentBundleAggregate.Create(id, name, BaseTime);

        var evt = Assert.IsType<DocumentBundleCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.BundleId);
        Assert.Equal(name, evt.Name);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = DocumentBundleAggregate.Create(id, new BundleName("Q1 Bundle"), BaseTime);

        Assert.Equal(id, aggregate.BundleId);
        Assert.Equal(BundleStatus.Open, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var b1 = DocumentBundleAggregate.Create(id, new BundleName("Bundle"), BaseTime);
        var b2 = DocumentBundleAggregate.Create(id, new BundleName("Bundle"), BaseTime);

        Assert.Equal(
            ((DocumentBundleCreatedEvent)b1.DomainEvents[0]).BundleId.Value,
            ((DocumentBundleCreatedEvent)b2.DomainEvents[0]).BundleId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentBundleState()
    {
        var id = NewId("History");
        var name = new BundleName("History Bundle");

        var history = new object[]
        {
            new DocumentBundleCreatedEvent(id, name, BaseTime)
        };

        var aggregate = (DocumentBundleAggregate)Activator.CreateInstance(typeof(DocumentBundleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.BundleId);
        Assert.Equal(BundleStatus.Open, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
