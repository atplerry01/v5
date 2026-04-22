using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed class IngestSessionAggregate : AggregateRoot
{
    public IngestSessionId SessionId { get; private set; }
    public BroadcastRef BroadcastRef { get; private set; }
    public IngestEndpoint Endpoint { get; private set; }
    public IngestSessionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public Timestamp AuthenticatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private IngestSessionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static IngestSessionAggregate Authenticate(
        IngestSessionId sessionId,
        BroadcastRef broadcastRef,
        IngestEndpoint endpoint,
        Timestamp authenticatedAt)
    {
        var aggregate = new IngestSessionAggregate();
        aggregate.RaiseDomainEvent(new IngestSessionAuthenticatedEvent(sessionId, broadcastRef, endpoint, authenticatedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void StartStreaming(Timestamp startedAt)
    {
        if (IsTerminal(Status))
            throw IngestSessionErrors.SessionAlreadyTerminal();

        if (Status != IngestSessionStatus.Authenticated)
            throw IngestSessionErrors.CannotStartUnlessAuthenticated();

        RaiseDomainEvent(new IngestStreamingStartedEvent(SessionId, startedAt));
    }

    public void Stall(Timestamp stalledAt)
    {
        if (IsTerminal(Status))
            throw IngestSessionErrors.SessionAlreadyTerminal();

        if (Status != IngestSessionStatus.Streaming)
            throw IngestSessionErrors.CannotStallUnlessStreaming();

        RaiseDomainEvent(new IngestSessionStalledEvent(SessionId, stalledAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (IsTerminal(Status))
            throw IngestSessionErrors.SessionAlreadyTerminal();

        if (Status != IngestSessionStatus.Stalled)
            throw IngestSessionErrors.CannotResumeUnlessStalled();

        RaiseDomainEvent(new IngestSessionResumedEvent(SessionId, resumedAt));
    }

    public void End(Timestamp endedAt)
    {
        if (IsTerminal(Status))
            throw IngestSessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new IngestSessionEndedEvent(SessionId, endedAt));
    }

    public void Fail(string reason, Timestamp failedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw IngestSessionErrors.EmptyFailureReason();

        if (IsTerminal(Status))
            throw IngestSessionErrors.SessionAlreadyTerminal();

        RaiseDomainEvent(new IngestSessionFailedEvent(SessionId, reason.Trim(), failedAt));
    }

    private static bool IsTerminal(IngestSessionStatus status) =>
        status == IngestSessionStatus.Ended || status == IngestSessionStatus.Failed;

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IngestSessionAuthenticatedEvent e:
                SessionId = e.SessionId;
                BroadcastRef = e.BroadcastRef;
                Endpoint = e.Endpoint;
                Status = IngestSessionStatus.Authenticated;
                AuthenticatedAt = e.AuthenticatedAt;
                LastModifiedAt = e.AuthenticatedAt;
                break;

            case IngestStreamingStartedEvent e:
                Status = IngestSessionStatus.Streaming;
                LastModifiedAt = e.StartedAt;
                break;

            case IngestSessionStalledEvent e:
                Status = IngestSessionStatus.Stalled;
                LastModifiedAt = e.StalledAt;
                break;

            case IngestSessionResumedEvent e:
                Status = IngestSessionStatus.Streaming;
                LastModifiedAt = e.ResumedAt;
                break;

            case IngestSessionEndedEvent e:
                Status = IngestSessionStatus.Ended;
                LastModifiedAt = e.EndedAt;
                break;

            case IngestSessionFailedEvent e:
                Status = IngestSessionStatus.Failed;
                FailureReason = e.FailureReason;
                LastModifiedAt = e.FailedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SessionId.Value == Guid.Empty)
            throw IngestSessionErrors.MissingSessionId();

        if (BroadcastRef.Value == Guid.Empty)
            throw IngestSessionErrors.MissingBroadcastRef();
    }
}
