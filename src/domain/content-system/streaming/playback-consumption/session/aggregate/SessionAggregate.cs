using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public sealed class SessionAggregate : AggregateRoot
{
    public SessionId SessionId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public SessionWindow Window { get; private set; }
    public SessionStatus Status { get; private set; }
    public SessionTerminationReason? TerminationReason { get; private set; }
    public Timestamp OpenedAt { get; private set; }
    public Timestamp? ClosedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private SessionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static SessionAggregate Open(
        SessionId sessionId,
        StreamRef streamRef,
        SessionWindow window,
        Timestamp openedAt)
    {
        if (window.HasExpired(openedAt))
            throw SessionErrors.OpenedAfterExpiry();

        var aggregate = new SessionAggregate();

        aggregate.RaiseDomainEvent(new SessionOpenedEvent(
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
            throw SessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Opened)
            throw SessionErrors.CannotActivateUnlessOpened();

        RaiseDomainEvent(new SessionActivatedEvent(SessionId, activatedAt));
    }

    public void Suspend(Timestamp suspendedAt)
    {
        if (IsTerminal(Status))
            throw SessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Active)
            throw SessionErrors.CannotSuspendUnlessActive();

        RaiseDomainEvent(new SessionSuspendedEvent(SessionId, suspendedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (IsTerminal(Status))
            throw SessionErrors.SessionAlreadyTerminal();

        if (Status != SessionStatus.Suspended)
            throw SessionErrors.CannotResumeUnlessSuspended();

        RaiseDomainEvent(new SessionResumedEvent(SessionId, resumedAt));
    }

    public void Close(SessionTerminationReason reason, Timestamp closedAt)
    {
        if (IsTerminal(Status))
            throw SessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new SessionClosedEvent(SessionId, reason, closedAt));
    }

    public void Fail(SessionTerminationReason reason, Timestamp failedAt)
    {
        if (IsTerminal(Status))
            throw SessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new SessionFailedEvent(SessionId, reason, failedAt));
    }

    public void Expire(Timestamp expiredAt)
    {
        if (IsTerminal(Status))
            throw SessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new SessionExpiredEvent(SessionId, expiredAt));
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
            case SessionOpenedEvent e:
                SessionId = e.SessionId;
                StreamRef = e.StreamRef;
                Window = e.Window;
                Status = SessionStatus.Opened;
                OpenedAt = e.OpenedAt;
                LastModifiedAt = e.OpenedAt;
                break;

            case SessionActivatedEvent e:
                Status = SessionStatus.Active;
                LastModifiedAt = e.ActivatedAt;
                break;

            case SessionSuspendedEvent e:
                Status = SessionStatus.Suspended;
                LastModifiedAt = e.SuspendedAt;
                break;

            case SessionResumedEvent e:
                Status = SessionStatus.Active;
                LastModifiedAt = e.ResumedAt;
                break;

            case SessionClosedEvent e:
                Status = SessionStatus.Closed;
                TerminationReason = e.Reason;
                ClosedAt = e.ClosedAt;
                LastModifiedAt = e.ClosedAt;
                break;

            case SessionFailedEvent e:
                Status = SessionStatus.Failed;
                TerminationReason = e.Reason;
                ClosedAt = e.FailedAt;
                LastModifiedAt = e.FailedAt;
                break;

            case SessionExpiredEvent e:
                Status = SessionStatus.Expired;
                ClosedAt = e.ExpiredAt;
                LastModifiedAt = e.ExpiredAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw SessionErrors.OrphanedSession();
    }
}
