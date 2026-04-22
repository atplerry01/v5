using Whycespace.Domain.ContentSystem.Document.Intake.Upload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.Intake.Upload;

public sealed class DocumentUploadAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static DocumentUploadId NewId(string seed) =>
        new(IdGen.Generate($"DocumentUploadAggregateTests:{seed}:upload"));

    [Fact]
    public void Request_RaisesDocumentUploadRequestedEvent()
    {
        var id = NewId("Request_Valid");
        var sourceRef = new DocumentUploadSourceRef(IdGen.Generate("DocumentUploadAggregateTests:source-ref"));
        var inputRef = new DocumentUploadInputRef(IdGen.Generate("DocumentUploadAggregateTests:input-ref"));

        var aggregate = DocumentUploadAggregate.Request(id, sourceRef, inputRef, BaseTime);

        var evt = Assert.IsType<DocumentUploadRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.UploadId);
        Assert.Equal(sourceRef, evt.SourceRef);
        Assert.Equal(inputRef, evt.InputRef);
    }

    [Fact]
    public void Request_SetsStatusToRequested()
    {
        var id = NewId("Request_State");
        var sourceRef = new DocumentUploadSourceRef(IdGen.Generate("DocumentUploadAggregateTests:source-state"));
        var inputRef = new DocumentUploadInputRef(IdGen.Generate("DocumentUploadAggregateTests:input-state"));

        var aggregate = DocumentUploadAggregate.Request(id, sourceRef, inputRef, BaseTime);

        Assert.Equal(id, aggregate.UploadId);
        Assert.Equal(DocumentUploadStatus.Requested, aggregate.Status);
    }

    [Fact]
    public void Request_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var sourceRef = new DocumentUploadSourceRef(IdGen.Generate("DocumentUploadAggregateTests:stable-source"));
        var inputRef = new DocumentUploadInputRef(IdGen.Generate("DocumentUploadAggregateTests:stable-input"));

        var a1 = DocumentUploadAggregate.Request(id, sourceRef, inputRef, BaseTime);
        var a2 = DocumentUploadAggregate.Request(id, sourceRef, inputRef, BaseTime);

        Assert.Equal(
            ((DocumentUploadRequestedEvent)a1.DomainEvents[0]).UploadId.Value,
            ((DocumentUploadRequestedEvent)a2.DomainEvents[0]).UploadId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentUploadState()
    {
        var id = NewId("History");
        var sourceRef = new DocumentUploadSourceRef(IdGen.Generate("DocumentUploadAggregateTests:history-source"));
        var inputRef = new DocumentUploadInputRef(IdGen.Generate("DocumentUploadAggregateTests:history-input"));

        var history = new object[]
        {
            new DocumentUploadRequestedEvent(id, sourceRef, inputRef, BaseTime)
        };

        var aggregate = (DocumentUploadAggregate)Activator.CreateInstance(typeof(DocumentUploadAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.UploadId);
        Assert.Equal(DocumentUploadStatus.Requested, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
