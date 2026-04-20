using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public sealed class BroadcastAggregate : AggregateRoot
{
    public BroadcastId BroadcastId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public BroadcastStatus Status { get; private set; }
    public BroadcastWindow? Window { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? EndedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private BroadcastAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static BroadcastAggregate Create(
        BroadcastId liveStreamId,
        StreamRef streamRef,
        Timestamp createdAt)
    {
        var aggregate = new BroadcastAggregate();

        aggregate.RaiseDomainEvent(new BroadcastCreatedEvent(liveStreamId, streamRef, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Schedule(BroadcastWindow window, Timestamp scheduledAt)
    {
        if (IsTerminal(Status))
            throw BroadcastErrors.CannotStartTerminal();

        if (Status == BroadcastStatus.Live || Status == BroadcastStatus.Paused)
            throw BroadcastErrors.CannotScheduleAfterStart();

        RaiseDomainEvent(new BroadcastScheduledEvent(BroadcastId, window, scheduledAt));
    }

    public void Start(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw BroadcastErrors.CannotStartTerminal();

        if (Status == BroadcastStatus.Live)
            throw BroadcastErrors.CannotStartLive();

        if (Status == BroadcastStatus.Paused)
            throw BroadcastErrors.CannotStartLive();

        RaiseDomainEvent(new BroadcastStartedEvent(BroadcastId, startedAt));
    }

    public void Pause(Timestamp pausedAt)
    {
        if (Status != BroadcastStatus.Live)
            throw BroadcastErrors.CannotPauseUnlessLive();

        RaiseDomainEvent(new BroadcastPausedEvent(BroadcastId, pausedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status != BroadcastStatus.Paused)
            throw BroadcastErrors.CannotResumeUnlessPaused();

        RaiseDomainEvent(new BroadcastResumedEvent(BroadcastId, resumedAt));
    }

    public void End(Timestamp endedAt)
    {
        if (Status != BroadcastStatus.Live && Status != BroadcastStatus.Paused)
            throw BroadcastErrors.CannotEndUnlessActive();

        RaiseDomainEvent(new BroadcastEndedEvent(BroadcastId, endedAt));
    }

    public void Cancel(string reason, Timestamp cancelledAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw BroadcastErrors.InvalidCancellationReason();

        if (IsTerminal(Status))
            throw BroadcastErrors.CannotCancelTerminal();

        RaiseDomainEvent(new BroadcastCancelledEvent(BroadcastId, reason.Trim(), cancelledAt));
    }

    private static bool IsTerminal(BroadcastStatus status) =>
        status == BroadcastStatus.Ended || status == BroadcastStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case BroadcastCreatedEvent e:
                BroadcastId = e.BroadcastId;
                StreamRef = e.StreamRef;
                Status = BroadcastStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case BroadcastScheduledEvent e:
                Window = e.Window;
                Status = BroadcastStatus.Scheduled;
                LastModifiedAt = e.ScheduledAt;
                break;

            case BroadcastStartedEvent e:
                Status = BroadcastStatus.Live;
                StartedAt ??= e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case BroadcastPausedEvent e:
                Status = BroadcastStatus.Paused;
                LastModifiedAt = e.PausedAt;
                break;

            case BroadcastResumedEvent e:
                Status = BroadcastStatus.Live;
                LastModifiedAt = e.ResumedAt;
                break;

            case BroadcastEndedEvent e:
                Status = BroadcastStatus.Ended;
                EndedAt = e.EndedAt;
                LastModifiedAt = e.EndedAt;
                break;

            case BroadcastCancelledEvent e:
                Status = BroadcastStatus.Cancelled;
                EndedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw BroadcastErrors.OrphanedLiveStream();
    }
}
