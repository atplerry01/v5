using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public sealed class DocumentUploadAggregate : AggregateRoot
{
    public DocumentUploadId UploadId { get; private set; }
    public DocumentUploadSourceRef SourceRef { get; private set; }
    public DocumentUploadInputRef InputRef { get; private set; }
    public DocumentUploadOutputRef? OutputRef { get; private set; }
    public DocumentUploadStatus Status { get; private set; }
    public DocumentUploadFailureReason? FailureReason { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp? AcceptedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentUploadAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentUploadAggregate Request(
        DocumentUploadId uploadId,
        DocumentUploadSourceRef sourceRef,
        DocumentUploadInputRef inputRef,
        Timestamp requestedAt)
    {
        var aggregate = new DocumentUploadAggregate();

        aggregate.RaiseDomainEvent(new DocumentUploadRequestedEvent(
            uploadId,
            sourceRef,
            inputRef,
            requestedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Accept(Timestamp acceptedAt)
    {
        if (IsTerminal(Status))
            throw DocumentUploadErrors.AlreadyTerminal();

        if (Status != DocumentUploadStatus.Requested)
            throw DocumentUploadErrors.CannotAcceptUnlessRequested();

        RaiseDomainEvent(new DocumentUploadAcceptedEvent(UploadId, acceptedAt));
    }

    public void StartProcessing(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw DocumentUploadErrors.AlreadyTerminal();

        if (Status != DocumentUploadStatus.Accepted)
            throw DocumentUploadErrors.CannotStartUnlessAccepted();

        RaiseDomainEvent(new DocumentUploadProcessingStartedEvent(UploadId, startedAt));
    }

    public void Complete(DocumentUploadOutputRef outputRef, Timestamp completedAt)
    {
        if (Status != DocumentUploadStatus.Processing)
            throw DocumentUploadErrors.CannotCompleteUnlessProcessing();

        RaiseDomainEvent(new DocumentUploadCompletedEvent(UploadId, outputRef, completedAt));
    }

    public void Fail(DocumentUploadFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw DocumentUploadErrors.AlreadyTerminal();

        RaiseDomainEvent(new DocumentUploadFailedEvent(UploadId, reason, failedAt));
    }

    public void Cancel(Timestamp cancelledAt)
    {
        if (IsTerminal(Status))
            throw DocumentUploadErrors.CannotCancelTerminal();

        RaiseDomainEvent(new DocumentUploadCancelledEvent(UploadId, cancelledAt));
    }

    private static bool IsTerminal(DocumentUploadStatus status) =>
        status == DocumentUploadStatus.Completed
        || status == DocumentUploadStatus.Failed
        || status == DocumentUploadStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentUploadRequestedEvent e:
                UploadId = e.UploadId;
                SourceRef = e.SourceRef;
                InputRef = e.InputRef;
                Status = DocumentUploadStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case DocumentUploadAcceptedEvent e:
                Status = DocumentUploadStatus.Accepted;
                AcceptedAt = e.AcceptedAt;
                LastModifiedAt = e.AcceptedAt;
                break;

            case DocumentUploadProcessingStartedEvent e:
                Status = DocumentUploadStatus.Processing;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case DocumentUploadCompletedEvent e:
                Status = DocumentUploadStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case DocumentUploadFailedEvent e:
                Status = DocumentUploadStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case DocumentUploadCancelledEvent e:
                Status = DocumentUploadStatus.Cancelled;
                CompletedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }
}
