namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public readonly record struct BookingId
{
    public Guid Value { get; }

    public BookingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BookingId value must not be empty.", nameof(value));
        Value = value;
    }
}
