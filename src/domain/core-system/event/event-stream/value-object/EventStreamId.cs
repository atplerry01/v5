namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public readonly record struct EventStreamId
{
    public Guid Value { get; }

    public EventStreamId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventStreamId value must not be empty.", nameof(value));
        Value = value;
    }
}
