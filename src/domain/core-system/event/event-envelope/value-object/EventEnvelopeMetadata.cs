namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public readonly record struct EventEnvelopeMetadata
{
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public string EventName { get; }

    public EventEnvelopeMetadata(Guid correlationId, Guid causationId, string eventName)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId must not be empty.", nameof(correlationId));

        if (causationId == Guid.Empty)
            throw new ArgumentException("CausationId must not be empty.", nameof(causationId));

        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("EventName must not be empty.", nameof(eventName));

        CorrelationId = correlationId;
        CausationId = causationId;
        EventName = eventName;
    }
}
