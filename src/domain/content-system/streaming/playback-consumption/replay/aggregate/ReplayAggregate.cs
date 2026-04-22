using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public sealed class ReplayAggregate : AggregateRoot
{
    public ReplayId ReplayId { get; private set; }
    public ArchiveRef ArchiveRef { get; private set; }
    public ViewerRef ViewerRef { get; private set; }
    public PlaybackPosition Position { get; private set; }
    public ReplayStatus Status { get; private set; }
    public Timestamp RequestedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ReplayAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ReplayAggregate Request(
        ReplayId replayId,
        ArchiveRef archiveRef,
        ViewerRef viewerRef,
        Timestamp requestedAt)
    {
        var aggregate = new ReplayAggregate();
        aggregate.RaiseDomainEvent(new ReplayRequestedEvent(replayId, archiveRef, viewerRef, requestedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Start(PlaybackPosition position, Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw ReplayErrors.ReplayAlreadyTerminal();

        if (Status != ReplayStatus.Requested)
            throw ReplayErrors.CannotStartUnlessRequested();

        RaiseDomainEvent(new ReplayStartedEvent(ReplayId, position, startedAt));
    }

    public void Pause(PlaybackPosition position, Timestamp pausedAt)
    {
        if (IsTerminal(Status))
            throw ReplayErrors.ReplayAlreadyTerminal();

        if (Status != ReplayStatus.Active)
            throw ReplayErrors.CannotPauseUnlessActive();

        RaiseDomainEvent(new ReplayPausedEvent(ReplayId, position, pausedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (IsTerminal(Status))
            throw ReplayErrors.ReplayAlreadyTerminal();

        if (Status != ReplayStatus.Paused)
            throw ReplayErrors.CannotResumeUnlessPaused();

        RaiseDomainEvent(new ReplayResumedEvent(ReplayId, resumedAt));
    }

    public void Complete(PlaybackPosition position, Timestamp completedAt)
    {
        if (IsTerminal(Status))
            throw ReplayErrors.ReplayAlreadyTerminal();

        if (Status != ReplayStatus.Active)
            throw ReplayErrors.CannotCompleteUnlessActive();

        RaiseDomainEvent(new ReplayCompletedEvent(ReplayId, position, completedAt));
    }

    public void Abandon(Timestamp abandonedAt)
    {
        if (IsTerminal(Status))
            throw ReplayErrors.ReplayAlreadyTerminal();

        RaiseDomainEvent(new ReplayAbandonedEvent(ReplayId, abandonedAt));
    }

    private static bool IsTerminal(ReplayStatus status) =>
        status == ReplayStatus.Completed || status == ReplayStatus.Abandoned;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReplayRequestedEvent e:
                ReplayId = e.ReplayId;
                ArchiveRef = e.ArchiveRef;
                ViewerRef = e.ViewerRef;
                Position = new PlaybackPosition(0);
                Status = ReplayStatus.Requested;
                RequestedAt = e.RequestedAt;
                LastModifiedAt = e.RequestedAt;
                break;

            case ReplayStartedEvent e:
                Position = e.Position;
                Status = ReplayStatus.Active;
                LastModifiedAt = e.StartedAt;
                break;

            case ReplayPausedEvent e:
                Position = e.Position;
                Status = ReplayStatus.Paused;
                LastModifiedAt = e.PausedAt;
                break;

            case ReplayResumedEvent e:
                Status = ReplayStatus.Active;
                LastModifiedAt = e.ResumedAt;
                break;

            case ReplayCompletedEvent e:
                Position = e.Position;
                Status = ReplayStatus.Completed;
                LastModifiedAt = e.CompletedAt;
                break;

            case ReplayAbandonedEvent e:
                Status = ReplayStatus.Abandoned;
                LastModifiedAt = e.AbandonedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (ReplayId.Value == Guid.Empty)
            throw ReplayErrors.MissingReplayId();

        if (ArchiveRef.Value == Guid.Empty)
            throw ReplayErrors.MissingArchiveRef();
    }
}
