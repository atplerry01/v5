using Whycespace.Domain.SharedKernel;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class ResourceReservationAggregate : AggregateRoot
{
    public ReservationId ReservationId { get; private set; }
    public ResourceId ResourceId { get; private set; } = null!;
    public decimal ReservedAmount { get; private set; }
    public DateTimeOffset ExpiryTime { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Active;

    public ResourceReservationAggregate() { }

    public void Create(Guid id)
    {
        Id = id;
        ReservationId = new ReservationId(id);
    }

    public static ResourceReservationAggregate Reserve(
        ResourceId resourceId,
        decimal amount,
        DateTimeOffset expiryTime,
        decimal availableInventory,
        DateTimeOffset now)
    {
        var spec = new ReservationWithinInventorySpec();
        if (!spec.IsSatisfiedBy(amount, availableInventory))
            throw new InvalidOperationException(ReservationErrors.ExceedsInventory);

        if (expiryTime <= now)
            throw new ArgumentException("Expiry time must be in the future.", nameof(expiryTime));

        var reservation = new ResourceReservationAggregate
        {
            ReservationId = ReservationId.FromSeed($"ResourceReservation:{resourceId}:{amount}:{expiryTime:O}"),
            ResourceId = resourceId,
            ReservedAmount = amount,
            ExpiryTime = expiryTime
        };

        reservation.Id = reservation.ReservationId;

        reservation.RaiseDomainEvent(new ResourceReservedEvent(
            reservation.ReservationId,
            resourceId,
            amount,
            expiryTime));

        return reservation;
    }

    public void Release()
    {
        EnsureActive();
        Status = ReservationStatus.Cancelled;
    }

    public void Expire()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException(ReservationErrors.NotActive);

        Status = ReservationStatus.Expired;

        RaiseDomainEvent(new ReservationExpiredEvent(
            ReservationId,
            ResourceId));
    }

    private void EnsureActive()
    {
        if (Status == ReservationStatus.Expired)
            throw new InvalidOperationException(ReservationErrors.AlreadyExpired);
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException(ReservationErrors.AlreadyCancelled);
    }
}
