using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public sealed class MediaUploadAggregate : AggregateRoot
{
    public MediaUploadId UploadId { get; private set; }
    public MediaUploadSourceRef SourceRef { get; private set; }
    public MediaUploadInputRef InputRef { get; private set; }
    public MediaUploadOutputRef? OutputRef { get; private set; }
    public MediaUploadStatus Status { get; private set; }
    public MediaUploadFailureReason? FailureReason { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp? AcceptedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MediaUploadAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaUploadAggregate Request(
        MediaUploadId uploadId,
        MediaUploadSourceRef sourceRef,
        MediaUploadInputRef inputRef,
        Timestamp requestedAt)
    {
        var aggregate = new MediaUploadAggregate();

        aggregate.RaiseDomainEvent(new MediaUploadRequestedEvent(
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
            throw MediaUploadErrors.AlreadyTerminal();

        if (Status != MediaUploadStatus.Requested)
            throw MediaUploadErrors.CannotAcceptUnlessRequested();

        RaiseDomainEvent(new MediaUploadAcceptedEvent(UploadId, acceptedAt));
    }

    public void StartProcessing(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw MediaUploadErrors.AlreadyTerminal();

        if (Status != MediaUploadStatus.Accepted)
            throw MediaUploadErrors.CannotStartUnlessAccepted();

        RaiseDomainEvent(new MediaUploadProcessingStartedEvent(UploadId, startedAt));
    }

    public void Complete(MediaUploadOutputRef outputRef, Timestamp completedAt)
    {
        if (Status != MediaUploadStatus.Processing)
            throw MediaUploadErrors.CannotCompleteUnlessProcessing();

        RaiseDomainEvent(new MediaUploadCompletedEvent(UploadId, outputRef, completedAt));
    }

    public void Fail(MediaUploadFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw MediaUploadErrors.AlreadyTerminal();

        RaiseDomainEvent(new MediaUploadFailedEvent(UploadId, reason, failedAt));
    }

    public void Cancel(Timestamp cancelledAt)
    {
        if (IsTerminal(Status))
            throw MediaUploadErrors.CannotCancelTerminal();

        RaiseDomainEvent(new MediaUploadCancelledEvent(UploadId, cancelledAt));
    }

    private static bool IsTerminal(MediaUploadStatus status) =>
        status == MediaUploadStatus.Completed
        || status == MediaUploadStatus.Failed
        || status == MediaUploadStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaUploadRequestedEvent e:
                UploadId = e.UploadId;
                SourceRef = e.SourceRef;
                InputRef = e.InputRef;
                Status = MediaUploadStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case MediaUploadAcceptedEvent e:
                Status = MediaUploadStatus.Accepted;
                AcceptedAt = e.AcceptedAt;
                LastModifiedAt = e.AcceptedAt;
                break;

            case MediaUploadProcessingStartedEvent e:
                Status = MediaUploadStatus.Processing;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case MediaUploadCompletedEvent e:
                Status = MediaUploadStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case MediaUploadFailedEvent e:
                Status = MediaUploadStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case MediaUploadCancelledEvent e:
                Status = MediaUploadStatus.Cancelled;
                CompletedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }
}
