namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct ReservationExpiry
{
    public DateTimeOffset ExpiresAt { get; }

    public ReservationExpiry(DateTimeOffset expiresAt)
    {
        ExpiresAt = expiresAt;
    }

    public bool IsExpiredAt(DateTimeOffset at) => at >= ExpiresAt;
}
