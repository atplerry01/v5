using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.CoreObject.Document;

public sealed class DocumentAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentId NewId(string seed) =>
        new(IdGen.Generate($"DocumentAggregateTests:{seed}:document"));

    private static StructuralOwnerRef NewStructuralOwner(string seed) =>
        new(IdGen.Generate($"DocumentAggregateTests:{seed}:structural-owner"));

    private static BusinessOwnerRef NewBusinessOwner(string seed) =>
        new(BusinessOwnerKind.Agreement, IdGen.Generate($"DocumentAggregateTests:{seed}:business-owner"));

    [Fact]
    public void Create_RaisesDocumentCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var title = new DocumentTitle("Q1 Report");
        var structuralOwner = NewStructuralOwner("Create_Valid");
        var businessOwner = NewBusinessOwner("Create_Valid");

        var aggregate = DocumentAggregate.Create(
            id, title, DocumentType.Report, DocumentClassification.Internal,
            structuralOwner, businessOwner, BaseTime);

        var evt = Assert.IsType<DocumentCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.DocumentId);
        Assert.Equal(title, evt.Title);
        Assert.Equal(DocumentType.Report, evt.Type);
    }

    [Fact]
    public void Create_SetsStatusToDraft()
    {
        var id = NewId("Create_State");

        var aggregate = DocumentAggregate.Create(
            id, new DocumentTitle("Draft"), DocumentType.Generic, DocumentClassification.Public,
            NewStructuralOwner("Create_State"), NewBusinessOwner("Create_State"), BaseTime);

        Assert.Equal(id, aggregate.DocumentId);
        Assert.Equal(DocumentStatus.Draft, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var title = new DocumentTitle("Stable Doc");
        var d1 = DocumentAggregate.Create(id, title, DocumentType.Generic, DocumentClassification.Public,
            NewStructuralOwner("Stable"), NewBusinessOwner("Stable"), BaseTime);
        var d2 = DocumentAggregate.Create(id, title, DocumentType.Generic, DocumentClassification.Public,
            NewStructuralOwner("Stable"), NewBusinessOwner("Stable"), BaseTime);

        Assert.Equal(
            ((DocumentCreatedEvent)d1.DomainEvents[0]).DocumentId.Value,
            ((DocumentCreatedEvent)d2.DomainEvents[0]).DocumentId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentState()
    {
        var id = NewId("History");
        var title = new DocumentTitle("History Doc");
        var structuralOwner = NewStructuralOwner("History");
        var businessOwner = NewBusinessOwner("History");

        var history = new object[]
        {
            new DocumentCreatedEvent(id, title, DocumentType.Report, DocumentClassification.Internal,
                structuralOwner, businessOwner, BaseTime)
        };

        var aggregate = (DocumentAggregate)Activator.CreateInstance(typeof(DocumentAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.DocumentId);
        Assert.Equal(DocumentStatus.Draft, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
