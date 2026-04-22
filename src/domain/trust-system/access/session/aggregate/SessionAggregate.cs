using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed class SessionAggregate : AggregateRoot
{
    public SessionId SessionId { get; private set; }
    public SessionDescriptor Descriptor { get; private set; }
    public SessionStatus Status { get; private set; }

    private SessionAggregate() { }

    public static SessionAggregate Open(SessionId id, SessionDescriptor descriptor, Timestamp openedAt)
    {
        var aggregate = new SessionAggregate();
        aggregate.RaiseDomainEvent(new SessionOpenedEvent(id, descriptor, openedAt));
        return aggregate;
    }

    public void Expire()
    {
        if (Status != SessionStatus.Active)
            throw new DomainInvariantViolationException("Session can only be expired from Active status.");
        RaiseDomainEvent(new SessionExpiredEvent(SessionId));
    }

    public void Terminate()
    {
        if (Status != SessionStatus.Active)
            throw new DomainInvariantViolationException("Session can only be terminated from Active status.");
        RaiseDomainEvent(new SessionTerminatedEvent(SessionId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SessionOpenedEvent e:
                SessionId = e.SessionId;
                Descriptor = e.Descriptor;
                Status = SessionStatus.Active;
                break;
            case SessionExpiredEvent:
                Status = SessionStatus.Expired;
                break;
            case SessionTerminatedEvent:
                Status = SessionStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(SessionId == default, "Session identity must be established.");
        Guard.Against(Descriptor == default, "Session descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Session status is not a defined enum value.");
    }
}
