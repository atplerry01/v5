namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public readonly record struct ReservationItemId
{
    public Guid Value { get; }

    public ReservationItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReservationItemId value must not be empty.", nameof(value));
        Value = value;
    }
}
