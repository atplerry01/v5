namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public readonly record struct ReservedCapacity
{
    public int Value { get; }

    public ReservedCapacity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("ReservedCapacity must be greater than zero.", nameof(value));

        Value = value;
    }
}
