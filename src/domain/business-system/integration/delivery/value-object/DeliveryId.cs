namespace Whycespace.Domain.BusinessSystem.Integration.Delivery;

public readonly record struct DeliveryId
{
    public Guid Value { get; }

    public DeliveryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeliveryId must not be empty.", nameof(value));

        Value = value;
    }
}

public enum DeliveryStatus
{
    Scheduled,
    Dispatched,
    Confirmed,
    Failed
}

public readonly record struct DeliveryDescriptor
{
    public Guid TargetReference { get; }
    public string PayloadType { get; }

    public DeliveryDescriptor(Guid targetReference, string payloadType)
    {
        if (targetReference == Guid.Empty)
            throw new ArgumentException("TargetReference must not be empty.", nameof(targetReference));

        if (string.IsNullOrWhiteSpace(payloadType))
            throw new ArgumentException("PayloadType must not be empty.", nameof(payloadType));

        TargetReference = targetReference;
        PayloadType = payloadType;
    }
}
