using Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.Descriptor.Metadata;

public sealed class DocumentMetadataAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentMetadataId NewId(string seed) =>
        new(IdGen.Generate($"DocumentMetadataAggregateTests:{seed}:metadata"));

    [Fact]
    public void Create_RaisesDocumentMetadataCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var docRef = new DocumentRef(IdGen.Generate("DocumentMetadataAggregateTests:doc-ref"));

        var aggregate = DocumentMetadataAggregate.Create(id, docRef, BaseTime);

        var evt = Assert.IsType<DocumentMetadataCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.MetadataId);
        Assert.Equal(docRef, evt.DocumentRef);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var docRef = new DocumentRef(IdGen.Generate("DocumentMetadataAggregateTests:doc-state"));

        var aggregate = DocumentMetadataAggregate.Create(id, docRef, BaseTime);

        Assert.Equal(id, aggregate.MetadataId);
        Assert.Equal(MetadataStatus.Open, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var docRef = new DocumentRef(IdGen.Generate("DocumentMetadataAggregateTests:stable-doc"));
        var m1 = DocumentMetadataAggregate.Create(id, docRef, BaseTime);
        var m2 = DocumentMetadataAggregate.Create(id, docRef, BaseTime);

        Assert.Equal(
            ((DocumentMetadataCreatedEvent)m1.DomainEvents[0]).MetadataId.Value,
            ((DocumentMetadataCreatedEvent)m2.DomainEvents[0]).MetadataId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentMetadataState()
    {
        var id = NewId("History");
        var docRef = new DocumentRef(IdGen.Generate("DocumentMetadataAggregateTests:history-doc"));

        var history = new object[]
        {
            new DocumentMetadataCreatedEvent(id, docRef, BaseTime)
        };

        var aggregate = (DocumentMetadataAggregate)Activator.CreateInstance(typeof(DocumentMetadataAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.MetadataId);
        Assert.Equal(MetadataStatus.Open, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
