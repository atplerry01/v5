using Whycespace.Domain.ContentSystem.Document.CoreObject.Record;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.CoreObject.Record;

public sealed class DocumentRecordAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentRecordId NewId(string seed) =>
        new(IdGen.Generate($"DocumentRecordAggregateTests:{seed}:record"));

    [Fact]
    public void Create_RaisesDocumentRecordCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var docRef = new DocumentRef(IdGen.Generate("DocumentRecordAggregateTests:doc-ref"));

        var aggregate = DocumentRecordAggregate.Create(id, docRef, BaseTime);

        var evt = Assert.IsType<DocumentRecordCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RecordId);
        Assert.Equal(docRef, evt.DocumentRef);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");
        var docRef = new DocumentRef(IdGen.Generate("DocumentRecordAggregateTests:doc-state"));

        var aggregate = DocumentRecordAggregate.Create(id, docRef, BaseTime);

        Assert.Equal(id, aggregate.RecordId);
        Assert.Equal(RecordStatus.Open, aggregate.Status);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var docRef = new DocumentRef(IdGen.Generate("DocumentRecordAggregateTests:stable-doc"));
        var r1 = DocumentRecordAggregate.Create(id, docRef, BaseTime);
        var r2 = DocumentRecordAggregate.Create(id, docRef, BaseTime);

        Assert.Equal(
            ((DocumentRecordCreatedEvent)r1.DomainEvents[0]).RecordId.Value,
            ((DocumentRecordCreatedEvent)r2.DomainEvents[0]).RecordId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentRecordState()
    {
        var id = NewId("History");
        var docRef = new DocumentRef(IdGen.Generate("DocumentRecordAggregateTests:history-doc"));

        var history = new object[]
        {
            new DocumentRecordCreatedEvent(id, docRef, BaseTime)
        };

        var aggregate = (DocumentRecordAggregate)Activator.CreateInstance(typeof(DocumentRecordAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.RecordId);
        Assert.Equal(RecordStatus.Open, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
