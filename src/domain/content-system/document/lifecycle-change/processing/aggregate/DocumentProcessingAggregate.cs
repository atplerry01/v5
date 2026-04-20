using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed class DocumentProcessingAggregate : AggregateRoot
{
    public ProcessingJobId JobId { get; private set; }
    public ProcessingKind Kind { get; private set; }
    public ProcessingInputRef InputRef { get; private set; }
    public ProcessingOutputRef? OutputRef { get; private set; }
    public ProcessingStatus Status { get; private set; }
    public ProcessingFailureReason? FailureReason { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private DocumentProcessingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentProcessingAggregate Request(
        ProcessingJobId jobId,
        ProcessingKind kind,
        ProcessingInputRef inputRef,
        Timestamp requestedAt)
    {
        var aggregate = new DocumentProcessingAggregate();

        aggregate.RaiseDomainEvent(new DocumentProcessingRequestedEvent(
            jobId,
            kind,
            inputRef,
            requestedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Start(Timestamp startedAt)
    {
        if (Status == ProcessingStatus.Running)
            throw DocumentProcessingErrors.JobAlreadyStarted();

        if (Status != ProcessingStatus.Requested)
            throw DocumentProcessingErrors.JobNotRequested();

        RaiseDomainEvent(new DocumentProcessingStartedEvent(JobId, startedAt));
    }

    public void Complete(ProcessingOutputRef outputRef, Timestamp completedAt)
    {
        if (Status != ProcessingStatus.Running)
            throw DocumentProcessingErrors.JobNotRunning();

        RaiseDomainEvent(new DocumentProcessingCompletedEvent(JobId, outputRef, completedAt));
    }

    public void Fail(ProcessingFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw DocumentProcessingErrors.JobAlreadyTerminal();

        RaiseDomainEvent(new DocumentProcessingFailedEvent(JobId, reason, failedAt));
    }

    public void Cancel(Timestamp cancelledAt)
    {
        if (IsTerminal(Status))
            throw DocumentProcessingErrors.CannotCancelTerminal();

        RaiseDomainEvent(new DocumentProcessingCancelledEvent(JobId, cancelledAt));
    }

    private static bool IsTerminal(ProcessingStatus status) =>
        status == ProcessingStatus.Completed
        || status == ProcessingStatus.Failed
        || status == ProcessingStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentProcessingRequestedEvent e:
                JobId = e.JobId;
                Kind = e.Kind;
                InputRef = e.InputRef;
                Status = ProcessingStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case DocumentProcessingStartedEvent e:
                Status = ProcessingStatus.Running;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case DocumentProcessingCompletedEvent e:
                Status = ProcessingStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case DocumentProcessingFailedEvent e:
                Status = ProcessingStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case DocumentProcessingCancelledEvent e:
                Status = ProcessingStatus.Cancelled;
                CompletedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }
}
