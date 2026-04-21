using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed class EventEnvelopeAggregate : AggregateRoot
{
    public EventEnvelopeId Id { get; private set; }
    public EventEnvelopeMetadata Metadata { get; private set; }
    public EventEnvelopeStatus Status { get; private set; }

    // ── Factory ──────────────────────────────────────────────────

    public static EventEnvelopeAggregate Seal(
        EventEnvelopeId id,
        EventEnvelopeMetadata metadata)
    {
        var aggregate = new EventEnvelopeAggregate();
        if (aggregate.Version >= 0)
            throw EventEnvelopeErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EventEnvelopeSealedEvent(id, metadata));
        return aggregate;
    }

    // ── Publish ─────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, nameof(Publish));

        RaiseDomainEvent(new EventEnvelopePublishedEvent(Id));
    }

    // ── Acknowledge ──────────────────────────────────────────────

    public void Acknowledge()
    {
        var specification = new CanAcknowledgeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, nameof(Acknowledge));

        RaiseDomainEvent(new EventEnvelopeAcknowledgedEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventEnvelopeSealedEvent e:
                Id = e.EnvelopeId;
                Metadata = e.Metadata;
                Status = EventEnvelopeStatus.Sealed;
                break;
            case EventEnvelopePublishedEvent:
                Status = EventEnvelopeStatus.Published;
                break;
            case EventEnvelopeAcknowledgedEvent:
                Status = EventEnvelopeStatus.Acknowledged;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EventEnvelopeErrors.MissingId();

        if (Metadata == default)
            throw EventEnvelopeErrors.MissingMetadata();

        if (!Enum.IsDefined(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, "validate");
    }
}
