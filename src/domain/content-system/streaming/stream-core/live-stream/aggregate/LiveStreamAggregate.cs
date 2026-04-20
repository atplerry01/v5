using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public sealed class LiveStreamAggregate : AggregateRoot
{
    public LiveStreamId LiveStreamId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public LiveStreamStatus Status { get; private set; }
    public LiveBroadcastWindow? Window { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? EndedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private LiveStreamAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static LiveStreamAggregate Create(
        LiveStreamId liveStreamId,
        StreamRef streamRef,
        Timestamp createdAt)
    {
        var aggregate = new LiveStreamAggregate();

        aggregate.RaiseDomainEvent(new LiveStreamCreatedEvent(liveStreamId, streamRef, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Schedule(LiveBroadcastWindow window, Timestamp scheduledAt)
    {
        if (IsTerminal(Status))
            throw LiveStreamErrors.CannotStartTerminal();

        if (Status == LiveStreamStatus.Live || Status == LiveStreamStatus.Paused)
            throw LiveStreamErrors.CannotScheduleAfterStart();

        RaiseDomainEvent(new LiveStreamScheduledEvent(LiveStreamId, window, scheduledAt));
    }

    public void Start(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw LiveStreamErrors.CannotStartTerminal();

        if (Status == LiveStreamStatus.Live)
            throw LiveStreamErrors.CannotStartLive();

        if (Status == LiveStreamStatus.Paused)
            throw LiveStreamErrors.CannotStartLive();

        RaiseDomainEvent(new LiveStreamStartedEvent(LiveStreamId, startedAt));
    }

    public void Pause(Timestamp pausedAt)
    {
        if (Status != LiveStreamStatus.Live)
            throw LiveStreamErrors.CannotPauseUnlessLive();

        RaiseDomainEvent(new LiveStreamPausedEvent(LiveStreamId, pausedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status != LiveStreamStatus.Paused)
            throw LiveStreamErrors.CannotResumeUnlessPaused();

        RaiseDomainEvent(new LiveStreamResumedEvent(LiveStreamId, resumedAt));
    }

    public void End(Timestamp endedAt)
    {
        if (Status != LiveStreamStatus.Live && Status != LiveStreamStatus.Paused)
            throw LiveStreamErrors.CannotEndUnlessActive();

        RaiseDomainEvent(new LiveStreamEndedEvent(LiveStreamId, endedAt));
    }

    public void Cancel(string reason, Timestamp cancelledAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw LiveStreamErrors.InvalidCancellationReason();

        if (IsTerminal(Status))
            throw LiveStreamErrors.CannotCancelTerminal();

        RaiseDomainEvent(new LiveStreamCancelledEvent(LiveStreamId, reason.Trim(), cancelledAt));
    }

    private static bool IsTerminal(LiveStreamStatus status) =>
        status == LiveStreamStatus.Ended || status == LiveStreamStatus.Cancelled;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LiveStreamCreatedEvent e:
                LiveStreamId = e.LiveStreamId;
                StreamRef = e.StreamRef;
                Status = LiveStreamStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case LiveStreamScheduledEvent e:
                Window = e.Window;
                Status = LiveStreamStatus.Scheduled;
                LastModifiedAt = e.ScheduledAt;
                break;

            case LiveStreamStartedEvent e:
                Status = LiveStreamStatus.Live;
                StartedAt ??= e.StartedAt;
                LastModifiedAt = e.StartedAt;
                break;

            case LiveStreamPausedEvent e:
                Status = LiveStreamStatus.Paused;
                LastModifiedAt = e.PausedAt;
                break;

            case LiveStreamResumedEvent e:
                Status = LiveStreamStatus.Live;
                LastModifiedAt = e.ResumedAt;
                break;

            case LiveStreamEndedEvent e:
                Status = LiveStreamStatus.Ended;
                EndedAt = e.EndedAt;
                LastModifiedAt = e.EndedAt;
                break;

            case LiveStreamCancelledEvent e:
                Status = LiveStreamStatus.Cancelled;
                EndedAt = e.CancelledAt;
                LastModifiedAt = e.CancelledAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw LiveStreamErrors.OrphanedLiveStream();
    }
}
