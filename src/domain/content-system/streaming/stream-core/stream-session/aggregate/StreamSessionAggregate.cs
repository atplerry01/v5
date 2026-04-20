using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed class StreamSessionAggregate : AggregateRoot
{
    public StreamSessionId SessionId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public SessionWindow Window { get; private set; }
    public SessionStatus Status { get; private set; }
    public SessionTerminationReason? TerminationReason { get; private set; }
    public Timestamp OpenedAt { get; private set; }
    public Timestamp? ClosedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private StreamSessionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static StreamSessionAggregate Open(
        StreamSessionId sessionId,
        StreamRef streamRef,
        SessionWindow window,
        Timestamp openedAt)
    {
        if (window.HasExpired(openedAt))
            throw StreamSessionErrors.OpenedAfterExpiry();

        var aggregate = new StreamSessionAggregate();

        aggregate.RaiseDomainEvent(new StreamSessionOpenedEvent(
            sessionId,
            streamRef,
            window,
            openedAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Opened)
            throw StreamSessionErrors.CannotActivateUnlessOpened();

        RaiseDomainEvent(new StreamSessionActivatedEvent(SessionId, activatedAt));
    }

    public void Suspend(Timestamp suspendedAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Active)
            throw StreamSessionErrors.CannotSuspendUnlessActive();

        RaiseDomainEvent(new StreamSessionSuspendedEvent(SessionId, suspendedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Suspended)
            throw StreamSessionErrors.CannotResumeUnlessSuspended();

        RaiseDomainEvent(new StreamSessionResumedEvent(SessionId, resumedAt));
    }

    public void Close(SessionTerminationReason reason, Timestamp closedAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new StreamSessionClosedEvent(SessionId, reason, closedAt));
    }

    public void Fail(SessionTerminationReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new StreamSessionFailedEvent(SessionId, reason, failedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (IsTerminal(Status))
            throw StreamSessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new StreamSessionExpiredEvent(SessionId, expiredAt));
    }

    private static bool IsTerminal(SessionStatus status) =>
        status == SessionStatus.Closed
        || status == SessionStatus.Failed
        || status == SessionStatus.Expired;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StreamSessionOpenedEvent e:
                SessionId = e.SessionId;
                StreamRef = e.StreamRef;
                Window = e.Window;
                Status = SessionStatus.Opened;
                OpenedAt = e.OpenedAt;
                LastModifiedAt = e.OpenedAt;
                break;

            case StreamSessionActivatedEvent e:
                Status = SessionStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case StreamSessionSuspendedEvent e:
                Status = SessionStatus.Suspended;
                LastModifiedAt = e.SuspendedAt;
                break;

            case StreamSessionResumedEvent e:
                Status = SessionStatus.Active;
                LastModifiedAt = e.ResumedAt;
                break;

            case StreamSessionClosedEvent e:
                Status = SessionStatus.Closed;
                TerminationReason = e.Reason;
                ClosedAt = e.ClosedAt;
                LastModifiedAt = e.ClosedAt;
                break;

            case StreamSessionFailedEvent e:
                Status = SessionStatus.Failed;
                TerminationReason = e.Reason;
                ClosedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case StreamSessionExpiredEvent e:
                Status = SessionStatus.Expired;
                ClosedAt = e.ExpiredAt;
                LastModifiedAt = e.ExpiredAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw StreamSessionErrors.OrphanedSession();
    }
}
