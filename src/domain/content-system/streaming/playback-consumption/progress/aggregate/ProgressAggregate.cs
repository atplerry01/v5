using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public sealed class ProgressAggregate : AggregateRoot
{
    public ProgressId ProgressId { get; private set; }
    public SessionRef SessionRef { get; private set; }
    public PlaybackPosition Position { get; private set; }
    public ProgressStatus Status { get; private set; }
    public Timestamp TrackedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ProgressAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ProgressAggregate Track(
        ProgressId progressId,
        SessionRef sessionRef,
        PlaybackPosition position,
        Timestamp trackedAt)
    {
        var aggregate = new ProgressAggregate();
        aggregate.RaiseDomainEvent(new ProgressTrackedEvent(progressId, sessionRef, position, trackedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void UpdatePosition(PlaybackPosition position, Timestamp updatedAt)
    {
        if (Status == ProgressStatus.Terminated)
            throw ProgressErrors.ProgressAlreadyTerminated();

        RaiseDomainEvent(new PlaybackPositionUpdatedEvent(ProgressId, position, updatedAt));
    }

    public void Pause(PlaybackPosition position, Timestamp pausedAt)
    {
        if (Status == ProgressStatus.Terminated)
            throw ProgressErrors.ProgressAlreadyTerminated();

        if (Status != ProgressStatus.Tracking)
            throw ProgressErrors.CannotPauseUnlessTracking();

        RaiseDomainEvent(new PlaybackPausedEvent(ProgressId, position, pausedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status == ProgressStatus.Terminated)
            throw ProgressErrors.ProgressAlreadyTerminated();

        if (Status != ProgressStatus.Paused)
            throw ProgressErrors.CannotResumeUnlessPaused();

        RaiseDomainEvent(new PlaybackResumedEvent(ProgressId, resumedAt));
    }

    public void Terminate(Timestamp terminatedAt)
    {
        if (Status == ProgressStatus.Terminated)
            throw ProgressErrors.ProgressAlreadyTerminated();

        RaiseDomainEvent(new ProgressTerminatedEvent(ProgressId, terminatedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProgressTrackedEvent e:
                ProgressId = e.ProgressId;
                SessionRef = e.SessionRef;
                Position = e.Position;
                Status = ProgressStatus.Tracking;
                TrackedAt = e.TrackedAt;
                LastModifiedAt = e.TrackedAt;
                break;

            case PlaybackPositionUpdatedEvent e:
                Position = e.Position;
                LastModifiedAt = e.UpdatedAt;
                break;

            case PlaybackPausedEvent e:
                Position = e.Position;
                Status = ProgressStatus.Paused;
                LastModifiedAt = e.PausedAt;
                break;

            case PlaybackResumedEvent e:
                Status = ProgressStatus.Tracking;
                LastModifiedAt = e.ResumedAt;
                break;

            case ProgressTerminatedEvent e:
                Status = ProgressStatus.Terminated;
                LastModifiedAt = e.TerminatedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (ProgressId.Value == Guid.Empty)
            throw ProgressErrors.MissingProgressId();

        if (SessionRef.Value == Guid.Empty)
            throw ProgressErrors.MissingSessionRef();
    }
}
