using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventEnvelope;

public sealed record EventEnvelope
{
    public EventId EventId { get; }
    public EventType Type { get; }
    public DomainRoute Source { get; }
    public string CausationId { get; }
    public string CorrelationId { get; }
    public Timestamp OccurredAt { get; }
    public byte[] Payload { get; }

    public EventEnvelope(
        EventId eventId,
        EventType type,
        DomainRoute source,
        string causationId,
        string correlationId,
        Timestamp occurredAt,
        byte[] payload)
    {
        Guard.Against(!source.IsValid(), "EventEnvelope requires a valid Source DomainRoute.");
        Guard.Against(string.IsNullOrWhiteSpace(causationId), "EventEnvelope requires a non-empty CausationId.");
        Guard.Against(string.IsNullOrWhiteSpace(correlationId), "EventEnvelope requires a non-empty CorrelationId.");
        Guard.Against(payload is null || payload.Length == 0, "EventEnvelope requires a non-empty Payload.");

        EventId = eventId;
        Type = type;
        Source = source;
        CausationId = causationId;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
        Payload = payload;
    }
}
