using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public readonly record struct EventEnvelopeMetadata
{
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public string EventName { get; }

    public EventEnvelopeMetadata(Guid correlationId, Guid causationId, string eventName)
    {
        Guard.Against(correlationId == Guid.Empty, "CorrelationId must not be empty.");
        Guard.Against(causationId == Guid.Empty, "CausationId must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(eventName), "EventName must not be empty.");

        CorrelationId = correlationId;
        CausationId = causationId;
        EventName = eventName;
    }
}
