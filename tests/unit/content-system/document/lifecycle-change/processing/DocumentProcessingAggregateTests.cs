using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.LifecycleChange.Processing;

public sealed class DocumentProcessingAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ProcessingJobId NewId(string seed) =>
        new(IdGen.Generate($"DocumentProcessingAggregateTests:{seed}:job"));

    [Fact]
    public void Request_RaisesDocumentProcessingRequestedEvent()
    {
        var id = NewId("Request_Valid");
        var inputRef = new ProcessingInputRef(IdGen.Generate("DocumentProcessingAggregateTests:input-ref"));

        var aggregate = DocumentProcessingAggregate.Request(id, ProcessingKind.Ocr, inputRef, BaseTime);

        var evt = Assert.IsType<DocumentProcessingRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.JobId);
        Assert.Equal(ProcessingKind.Ocr, evt.Kind);
        Assert.Equal(inputRef, evt.InputRef);
    }

    [Fact]
    public void Request_SetsStatusToRequested()
    {
        var id = NewId("Request_State");
        var inputRef = new ProcessingInputRef(IdGen.Generate("DocumentProcessingAggregateTests:input-state"));

        var aggregate = DocumentProcessingAggregate.Request(id, ProcessingKind.TextExtraction, inputRef, BaseTime);

        Assert.Equal(id, aggregate.JobId);
        Assert.Equal(ProcessingStatus.Requested, aggregate.Status);
    }

    [Fact]
    public void Request_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var inputRef = new ProcessingInputRef(IdGen.Generate("DocumentProcessingAggregateTests:stable-input"));
        var p1 = DocumentProcessingAggregate.Request(id, ProcessingKind.Ocr, inputRef, BaseTime);
        var p2 = DocumentProcessingAggregate.Request(id, ProcessingKind.Ocr, inputRef, BaseTime);

        Assert.Equal(
            ((DocumentProcessingRequestedEvent)p1.DomainEvents[0]).JobId.Value,
            ((DocumentProcessingRequestedEvent)p2.DomainEvents[0]).JobId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesDocumentProcessingState()
    {
        var id = NewId("History");
        var inputRef = new ProcessingInputRef(IdGen.Generate("DocumentProcessingAggregateTests:history-input"));

        var history = new object[]
        {
            new DocumentProcessingRequestedEvent(id, ProcessingKind.Ocr, inputRef, BaseTime)
        };

        var aggregate = (DocumentProcessingAggregate)Activator.CreateInstance(typeof(DocumentProcessingAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.JobId);
        Assert.Equal(ProcessingStatus.Requested, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
