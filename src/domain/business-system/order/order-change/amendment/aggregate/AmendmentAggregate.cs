using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class AmendmentAggregate : AggregateRoot
{
    public AmendmentId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public AmendmentReason Reason { get; private set; }
    public AmendmentStatus Status { get; private set; }

    public static AmendmentAggregate Request(
        AmendmentId id,
        OrderRef order,
        AmendmentReason reason,
        DateTimeOffset requestedAt)
    {
        var aggregate = new AmendmentAggregate();
        if (aggregate.Version >= 0)
            throw AmendmentErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AmendmentRequestedEvent(id, order, reason, requestedAt));
        return aggregate;
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new AmendmentAcceptedEvent(Id, acceptedAt));
    }

    public void MarkApplied(DateTimeOffset appliedAt)
    {
        var specification = new CanApplySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(MarkApplied));

        RaiseDomainEvent(new AmendmentAppliedEvent(Id, appliedAt));
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new AmendmentRejectedEvent(Id, rejectedAt));
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new AmendmentCancelledEvent(Id, cancelledAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AmendmentRequestedEvent e:
                Id = e.AmendmentId;
                Order = e.Order;
                Reason = e.Reason;
                Status = AmendmentStatus.Requested;
                break;
            case AmendmentAcceptedEvent:
                Status = AmendmentStatus.Accepted;
                break;
            case AmendmentAppliedEvent:
                Status = AmendmentStatus.Applied;
                break;
            case AmendmentRejectedEvent:
                Status = AmendmentStatus.Rejected;
                break;
            case AmendmentCancelledEvent:
                Status = AmendmentStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AmendmentErrors.MissingId();

        if (Order == default)
            throw AmendmentErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, "validate");
    }
}
