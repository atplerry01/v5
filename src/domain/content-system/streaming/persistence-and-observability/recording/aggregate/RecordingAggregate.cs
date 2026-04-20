using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed class RecordingAggregate : AggregateRoot
{
    public RecordingId RecordingId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public StreamSessionRef? SessionRef { get; private set; }
    public RecordingStatus Status { get; private set; }
    public RecordingOutputRef? OutputRef { get; private set; }
    public RecordingFailureReason? FailureReason { get; private set; }
    public Timestamp StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private RecordingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RecordingAggregate Start(
        RecordingId recordingId,
        StreamRef streamRef,
        StreamSessionRef? sessionRef,
        Timestamp startedAt)
    {
        var aggregate = new RecordingAggregate();

        aggregate.RaiseDomainEvent(new RecordingStartedEvent(
            recordingId,
            streamRef,
            sessionRef,
            startedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Complete(RecordingOutputRef outputRef, Timestamp completedAt)
    {
        if (Status == RecordingStatus.Failed)
            throw RecordingErrors.CannotCompleteAfterFailure();

        if (Status != RecordingStatus.Started)
            throw RecordingErrors.CannotCompleteUnlessStarted();

        RaiseDomainEvent(new RecordingCompletedEvent(RecordingId, outputRef, completedAt));
    }

    public void Fail(RecordingFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw RecordingErrors.CannotFailTerminal();

        RaiseDomainEvent(new RecordingFailedEvent(RecordingId, reason, failedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == RecordingStatus.Finalized)
            throw RecordingErrors.AlreadyFinalized();

        if (Status != RecordingStatus.Completed)
            throw RecordingErrors.CannotFinalizeUnlessCompleted();

        RaiseDomainEvent(new RecordingFinalizedEvent(RecordingId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == RecordingStatus.Archived)
            throw RecordingErrors.AlreadyArchived();

        if (Status != RecordingStatus.Finalized)
            throw RecordingErrors.CannotArchiveUnlessFinalized();

        RaiseDomainEvent(new RecordingArchivedEvent(RecordingId, archivedAt));
    }

    private static bool IsTerminal(RecordingStatus status) =>
        status == RecordingStatus.Failed
        || status == RecordingStatus.Finalized
        || status == RecordingStatus.Archived;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RecordingStartedEvent e:
                RecordingId = e.RecordingId;
                StreamRef = e.StreamRef;
                SessionRef = e.SessionRef;
                Status = RecordingStatus.Started;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case RecordingCompletedEvent e:
                Status = RecordingStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case RecordingFailedEvent e:
                Status = RecordingStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case RecordingFinalizedEvent e:
                Status = RecordingStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case RecordingArchivedEvent e:
                Status = RecordingStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw RecordingErrors.OrphanedRecording();
    }
}
