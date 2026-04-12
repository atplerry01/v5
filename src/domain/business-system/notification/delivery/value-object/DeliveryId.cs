namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public readonly record struct DeliveryId
{
    public Guid Value { get; }

    public DeliveryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeliveryId value must not be empty.", nameof(value));

        Value = value;
    }
}