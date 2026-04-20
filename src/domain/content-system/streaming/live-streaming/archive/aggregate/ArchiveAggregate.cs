using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed class ArchiveAggregate : AggregateRoot
{
    public ArchiveId ArchiveId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public StreamSessionRef? SessionRef { get; private set; }
    public ArchiveStatus Status { get; private set; }
    public ArchiveOutputRef? OutputRef { get; private set; }
    public ArchiveFailureReason? FailureReason { get; private set; }
    public Timestamp StartedAt { get; private set; }
    public Timestamp? CompletedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ArchiveAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ArchiveAggregate Start(
        ArchiveId recordingId,
        StreamRef streamRef,
        StreamSessionRef? sessionRef,
        Timestamp startedAt)
    {
        var aggregate = new ArchiveAggregate();

        aggregate.RaiseDomainEvent(new ArchiveStartedEvent(
            recordingId,
            streamRef,
            sessionRef,
            startedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Complete(ArchiveOutputRef outputRef, Timestamp completedAt)
    {
        if (Status == ArchiveStatus.Failed)
            throw ArchiveErrors.CannotCompleteAfterFailure();

        if (Status != ArchiveStatus.Started)
            throw ArchiveErrors.CannotCompleteUnlessStarted();

        RaiseDomainEvent(new ArchiveCompletedEvent(ArchiveId, outputRef, completedAt));
    }

    public void Fail(ArchiveFailureReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw ArchiveErrors.CannotFailTerminal();

        RaiseDomainEvent(new ArchiveFailedEvent(ArchiveId, reason, failedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == ArchiveStatus.Finalized)
            throw ArchiveErrors.AlreadyFinalized();

        if (Status != ArchiveStatus.Completed)
            throw ArchiveErrors.CannotFinalizeUnlessCompleted();

        RaiseDomainEvent(new ArchiveFinalizedEvent(ArchiveId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == ArchiveStatus.Archived)
            throw ArchiveErrors.AlreadyArchived();

        if (Status != ArchiveStatus.Finalized)
            throw ArchiveErrors.CannotArchiveUnlessFinalized();

        RaiseDomainEvent(new ArchiveArchivedEvent(ArchiveId, archivedAt));
    }

    private static bool IsTerminal(ArchiveStatus status) =>
        status == ArchiveStatus.Failed
        || status == ArchiveStatus.Finalized
        || status == ArchiveStatus.Archived;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ArchiveStartedEvent e:
                ArchiveId = e.ArchiveId;
                StreamRef = e.StreamRef;
                SessionRef = e.SessionRef;
                Status = ArchiveStatus.Started;
                StartedAt = e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case ArchiveCompletedEvent e:
                Status = ArchiveStatus.Completed;
                OutputRef = e.OutputRef;
                CompletedAt = e.CompletedAt;
                LastModifiedAt = e.CompletedAt;
                break;

            case ArchiveFailedEvent e:
                Status = ArchiveStatus.Failed;
                FailureReason = e.Reason;
                CompletedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case ArchiveFinalizedEvent e:
                Status = ArchiveStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case ArchiveArchivedEvent e:
                Status = ArchiveStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw ArchiveErrors.OrphanedRecording();
    }
}
