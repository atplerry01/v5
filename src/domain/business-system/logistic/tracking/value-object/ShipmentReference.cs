namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public readonly record struct ShipmentReference
{
    public Guid Value { get; }

    public ShipmentReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ShipmentReference value must not be empty.", nameof(value));

        Value = value;
    }
}
