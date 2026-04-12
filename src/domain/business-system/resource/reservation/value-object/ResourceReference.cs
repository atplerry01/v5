namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public readonly record struct ResourceReference
{
    public Guid Value { get; }

    public ResourceReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ResourceReference value must not be empty.", nameof(value));

        Value = value;
    }
}
