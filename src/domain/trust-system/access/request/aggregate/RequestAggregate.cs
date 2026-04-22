using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed class RequestAggregate : AggregateRoot
{
    public RequestId RequestId { get; private set; }
    public RequestDescriptor Descriptor { get; private set; }
    public RequestStatus Status { get; private set; }

    private RequestAggregate() { }

    public static RequestAggregate Submit(RequestId id, RequestDescriptor descriptor, Timestamp submittedAt)
    {
        var aggregate = new RequestAggregate();
        aggregate.RaiseDomainEvent(new RequestSubmittedEvent(id, descriptor, submittedAt));
        return aggregate;
    }

    public void Approve()
    {
        if (Status != RequestStatus.Submitted)
            throw new DomainInvariantViolationException("Request can only be approved from Submitted status.");
        RaiseDomainEvent(new RequestApprovedEvent(RequestId));
    }

    public void Deny(string reason)
    {
        if (Status != RequestStatus.Submitted)
            throw new DomainInvariantViolationException("Request can only be denied from Submitted status.");
        Guard.Against(string.IsNullOrWhiteSpace(reason), "Denial reason must not be empty.");
        RaiseDomainEvent(new RequestDeniedEvent(RequestId, reason.Trim()));
    }

    public void Withdraw()
    {
        if (Status != RequestStatus.Submitted)
            throw new DomainInvariantViolationException("Request can only be withdrawn from Submitted status.");
        RaiseDomainEvent(new RequestWithdrawnEvent(RequestId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RequestSubmittedEvent e:
                RequestId = e.RequestId;
                Descriptor = e.Descriptor;
                Status = RequestStatus.Submitted;
                break;
            case RequestApprovedEvent:
                Status = RequestStatus.Approved;
                break;
            case RequestDeniedEvent:
                Status = RequestStatus.Denied;
                break;
            case RequestWithdrawnEvent:
                Status = RequestStatus.Withdrawn;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(RequestId == default, "Request identity must be established.");
        Guard.Against(Descriptor == default, "Request descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Request status is not a defined enum value.");
    }
}
