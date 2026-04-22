using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public sealed class EventMetadataAggregate : AggregateRoot
{
    public EventMetadataId EventMetadataId { get; private set; }
    public EventEnvelopeRef EnvelopeRef { get; private set; }
    public ExecutionHash ExecutionHash { get; private set; }
    public PolicyDecisionHash PolicyDecisionHash { get; private set; }
    public EventActorId ActorId { get; private set; }
    public EventTraceId TraceId { get; private set; }
    public EventSpanId SpanId { get; private set; }
    public Timestamp IssuedAt { get; private set; }

    private EventMetadataAggregate() { }

    public static EventMetadataAggregate Attach(
        EventMetadataId id,
        EventEnvelopeRef envelopeRef,
        ExecutionHash executionHash,
        PolicyDecisionHash policyDecisionHash,
        EventActorId actorId,
        EventTraceId traceId,
        EventSpanId spanId,
        Timestamp issuedAt)
    {
        var aggregate = new EventMetadataAggregate();
        if (aggregate.Version >= 0)
            throw EventMetadataErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EventMetadataAttachedEvent(
            id, envelopeRef, executionHash, policyDecisionHash, actorId, traceId, spanId, issuedAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventMetadataAttachedEvent e:
                EventMetadataId = e.EventMetadataId;
                EnvelopeRef = e.EnvelopeRef;
                ExecutionHash = e.ExecutionHash;
                PolicyDecisionHash = e.PolicyDecisionHash;
                ActorId = e.ActorId;
                TraceId = e.TraceId;
                SpanId = e.SpanId;
                IssuedAt = e.IssuedAt;
                break;
        }
    }
}
