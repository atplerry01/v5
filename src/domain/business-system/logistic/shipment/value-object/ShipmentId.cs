namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public readonly record struct ShipmentId
{
    public Guid Value { get; }

    public ShipmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ShipmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
