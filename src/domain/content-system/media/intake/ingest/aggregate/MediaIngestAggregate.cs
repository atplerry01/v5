using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed class MediaIngestAggregate : AggregateRoot
{
    public MediaIngestId UploadId { get; private set; }
    public MediaIngestSourceRef SourceRef { get; private set; }
    public MediaIngestInputRef InputRef { get; private set; }
    public MediaIngestOutputRef? OutputRef { get; private set; }
    public MediaIngestStatus Status { get; private set; }
    public MediaIngestFailureReason? FailureReason { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp? AcceptedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MediaIngestAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaIngestAggregate Request(
        MediaIngestId uploadId,
        MediaIngestSourceRef sourceRef,
        MediaIngestInputRef inputRef,
        Timestamp requestedAt)
    {
        var aggregate = new MediaIngestAggregate();

        aggregate.RaiseDomainEvent(new MediaIngestRequestedEvent(
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
            throw MediaIngestErrors.AlreadyTerminal();

        if (Status != MediaIngestStatus.Requested)
            throw MediaIngestErrors.CannotAcceptUnlessRequested();

        RaiseDomainEvent(new MediaIngestAcceptedEvent(UploadId, acceptedAt));
    }

    public void StartProcessing(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw MediaIngestErrors.AlreadyTerminal();

        if (Status != MediaIngestStatus.Accepted)
            throw MediaIngestErrors.CannotStartUnlessAccepted();

        RaiseDomainEvent(new MediaIngestProcessingStartedEvent(UploadId, startedAt));
    }

    public void Complete(MediaIngestOutputRef outputRef, Timestamp completedAt)
    {
        if (Status != MediaIngestStatus.Processing)
            throw MediaIngestErrors.CannotCompleteUnlessProcessing();

        RaiseDomainEvent(new MediaIngestCompletedEvent(UploadId, outputRef, completedAt));
    }

    public void Fail(MediaIngestFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw MediaIngestErrors.AlreadyTerminal();

        RaiseDomainEvent(new MediaIngestFailedEvent(UploadId, reason, failedAt));
    }

    public void Cancel(Timestamp cancelledAt)
    {
        if (IsTerminal(Status))
            throw MediaIngestErrors.CannotCancelTerminal();

        RaiseDomainEvent(new MediaIngestCancelledEvent(UploadId, cancelledAt));
    }

    private static bool IsTerminal(MediaIngestStatus status) =>
        status == MediaIngestStatus.Completed
        || status == MediaIngestStatus.Failed
        || status == MediaIngestStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaIngestRequestedEvent e:
                UploadId = e.UploadId;
                SourceRef = e.SourceRef;
                InputRef = e.InputRef;
                Status = MediaIngestStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case MediaIngestAcceptedEvent e:
                Status = MediaIngestStatus.Accepted;
                AcceptedAt = e.AcceptedAt;
                LastModifiedAt = e.AcceptedAt;
                break;

            case MediaIngestProcessingStartedEvent e:
                Status = MediaIngestStatus.Processing;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case MediaIngestCompletedEvent e:
                Status = MediaIngestStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case MediaIngestFailedEvent e:
                Status = MediaIngestStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case MediaIngestCancelledEvent e:
                Status = MediaIngestStatus.Cancelled;
                CompletedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }
}
