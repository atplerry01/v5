using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed class CancellationAggregate : AggregateRoot
{
    public CancellationId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public CancellationReason Reason { get; private set; }
    public CancellationStatus Status { get; private set; }

    public static CancellationAggregate Request(
        CancellationId id,
        OrderRef order,
        CancellationReason reason,
        DateTimeOffset requestedAt)
    {
        var aggregate = new CancellationAggregate();
        if (aggregate.Version >= 0)
            throw CancellationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new CancellationRequestedEvent(id, order, reason, requestedAt));
        return aggregate;
    }

    public void Confirm(DateTimeOffset confirmedAt)
    {
        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CancellationErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new CancellationConfirmedEvent(Id, confirmedAt));
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CancellationErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new CancellationRejectedEvent(Id, rejectedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CancellationRequestedEvent e:
                Id = e.CancellationId;
                Order = e.Order;
                Reason = e.Reason;
                Status = CancellationStatus.Requested;
                break;
            case CancellationConfirmedEvent:
                Status = CancellationStatus.Confirmed;
                break;
            case CancellationRejectedEvent:
                Status = CancellationStatus.Rejected;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CancellationErrors.MissingId();

        if (Order == default)
            throw CancellationErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw CancellationErrors.InvalidStateTransition(Status, "validate");
    }
}
