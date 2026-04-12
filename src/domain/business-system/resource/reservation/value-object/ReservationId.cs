namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public readonly record struct ReservationId
{
    public Guid Value { get; }

    public ReservationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReservationId value must not be empty.", nameof(value));

        Value = value;
    }
}
