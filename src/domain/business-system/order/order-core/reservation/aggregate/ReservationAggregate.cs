using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed class ReservationAggregate : AggregateRoot
{
    public ReservationId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemRef? LineItem { get; private set; }
    public ReservationSubjectRef Subject { get; private set; }
    public ReservationQuantity Quantity { get; private set; }
    public ReservationExpiry Expiry { get; private set; }
    public ReservationStatus Status { get; private set; }

    public static ReservationAggregate Hold(
        ReservationId id,
        OrderRef order,
        ReservationSubjectRef subject,
        ReservationQuantity quantity,
        ReservationExpiry expiry,
        DateTimeOffset heldAt,
        LineItemRef? lineItem = null)
    {
        if (expiry.IsExpiredAt(heldAt))
            throw ReservationErrors.ExpiryInPast();

        var aggregate = new ReservationAggregate();
        if (aggregate.Version >= 0)
            throw ReservationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ReservationHeldEvent(id, order, lineItem, subject, quantity, expiry, heldAt));
        return aggregate;
    }

    public void Confirm(DateTimeOffset confirmedAt)
    {
        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new ReservationConfirmedEvent(Id, confirmedAt));
    }

    public void Release(DateTimeOffset releasedAt)
    {
        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        RaiseDomainEvent(new ReservationReleasedEvent(Id, releasedAt));
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        if (!Expiry.IsExpiredAt(expiredAt))
            throw ReservationErrors.ExpiryNotReached();

        RaiseDomainEvent(new ReservationExpiredEvent(Id, expiredAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReservationHeldEvent e:
                Id = e.ReservationId;
                Order = e.Order;
                LineItem = e.LineItem;
                Subject = e.Subject;
                Quantity = e.Quantity;
                Expiry = e.Expiry;
                Status = ReservationStatus.Held;
                break;
            case ReservationConfirmedEvent:
                Status = ReservationStatus.Confirmed;
                break;
            case ReservationReleasedEvent:
                Status = ReservationStatus.Released;
                break;
            case ReservationExpiredEvent:
                Status = ReservationStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ReservationErrors.MissingId();

        if (Order == default)
            throw ReservationErrors.MissingOrderRef();

        if (Subject == default)
            throw ReservationErrors.MissingSubject();

        if (Quantity == default)
            throw ReservationErrors.MissingQuantity();

        if (Expiry == default)
            throw ReservationErrors.MissingExpiry();

        if (!Enum.IsDefined(Status))
            throw ReservationErrors.InvalidStateTransition(Status, "validate");
    }
}
