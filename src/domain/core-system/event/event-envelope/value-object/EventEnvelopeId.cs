namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public readonly record struct EventEnvelopeId
{
    public Guid Value { get; }

    public EventEnvelopeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventEnvelopeId cannot be empty.", nameof(value));

        Value = value;
    }
}
