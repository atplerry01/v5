using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public sealed class ReservationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReservationId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemRef? LineItem { get; private set; }
    public ReservationSubjectRef Subject { get; private set; }
    public ReservationQuantity Quantity { get; private set; }
    public ReservationExpiry Expiry { get; private set; }
    public ReservationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ReservationAggregate() { }

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

        var @event = new ReservationHeldEvent(id, order, lineItem, subject, quantity, expiry, heldAt);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm(DateTimeOffset confirmedAt)
    {
        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        var @event = new ReservationConfirmedEvent(Id, confirmedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Release(DateTimeOffset releasedAt)
    {
        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        var @event = new ReservationReleasedEvent(Id, releasedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.AlreadyTerminal(Id, Status);

        if (!Expiry.IsExpiredAt(expiredAt))
            throw ReservationErrors.ExpiryNotReached();

        var @event = new ReservationExpiredEvent(Id, expiredAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReservationHeldEvent @event)
    {
        Id = @event.ReservationId;
        Order = @event.Order;
        LineItem = @event.LineItem;
        Subject = @event.Subject;
        Quantity = @event.Quantity;
        Expiry = @event.Expiry;
        Status = ReservationStatus.Held;
        Version++;
    }

    private void Apply(ReservationConfirmedEvent @event)
    {
        Status = ReservationStatus.Confirmed;
        Version++;
    }

    private void Apply(ReservationReleasedEvent @event)
    {
        Status = ReservationStatus.Released;
        Version++;
    }

    private void Apply(ReservationExpiredEvent @event)
    {
        Status = ReservationStatus.Expired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ReservationErrors.MissingId();

        if (Order == default)
            throw ReservationErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw ReservationErrors.InvalidStateTransition(Status, "validate");
    }
}
