namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public readonly record struct EventBridgeId
{
    public Guid Value { get; }

    public EventBridgeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventBridgeId value must not be empty.", nameof(value));
        Value = value;
    }
}
