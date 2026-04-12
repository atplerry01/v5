namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public readonly record struct EventMappingId
{
    public Guid Value { get; }

    public EventMappingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventMappingId value must not be empty.", nameof(value));
        Value = value;
    }
}
