using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public sealed class MediaProcessingAggregate : AggregateRoot
{
    public MediaProcessingJobId JobId { get; private set; }
    public MediaProcessingKind Kind { get; private set; }
    public MediaProcessingInputRef InputRef { get; private set; }
    public MediaProcessingOutputRef? OutputRef { get; private set; }
    public MediaProcessingStatus Status { get; private set; }
    public MediaProcessingFailureReason? FailureReason { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private MediaProcessingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static MediaProcessingAggregate Request(
        MediaProcessingJobId jobId,
        MediaProcessingKind kind,
        MediaProcessingInputRef inputRef,
        Timestamp requestedAt)
    {
        var aggregate = new MediaProcessingAggregate();

        aggregate.RaiseDomainEvent(new MediaProcessingRequestedEvent(
            jobId,
            kind,
            inputRef,
            requestedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Start(Timestamp startedAt)
    {
        if (Status == MediaProcessingStatus.Running)
            throw MediaProcessingErrors.JobAlreadyStarted();

        if (Status != MediaProcessingStatus.Requested)
            throw MediaProcessingErrors.JobNotRequested();

        RaiseDomainEvent(new MediaProcessingStartedEvent(JobId, startedAt));
    }

    public void Complete(MediaProcessingOutputRef outputRef, Timestamp completedAt)
    {
        if (Status != MediaProcessingStatus.Running)
            throw MediaProcessingErrors.JobNotRunning();

        RaiseDomainEvent(new MediaProcessingCompletedEvent(JobId, outputRef, completedAt));
    }

    public void Fail(MediaProcessingFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw MediaProcessingErrors.JobAlreadyTerminal();

        RaiseDomainEvent(new MediaProcessingFailedEvent(JobId, reason, failedAt));
    }

    public void Cancel(Timestamp cancelledAt)
    {
        if (IsTerminal(Status))
            throw MediaProcessingErrors.CannotCancelTerminal();

        RaiseDomainEvent(new MediaProcessingCancelledEvent(JobId, cancelledAt));
    }

    private static bool IsTerminal(MediaProcessingStatus status) =>
        status == MediaProcessingStatus.Completed
        || status == MediaProcessingStatus.Failed
        || status == MediaProcessingStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MediaProcessingRequestedEvent e:
                JobId = e.JobId;
                Kind = e.Kind;
                InputRef = e.InputRef;
                Status = MediaProcessingStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case MediaProcessingStartedEvent e:
                Status = MediaProcessingStatus.Running;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case MediaProcessingCompletedEvent e:
                Status = MediaProcessingStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case MediaProcessingFailedEvent e:
                Status = MediaProcessingStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case MediaProcessingCancelledEvent e:
                Status = MediaProcessingStatus.Cancelled;
                CompletedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }
}
