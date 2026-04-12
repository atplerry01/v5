namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed class EventEnvelopeAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EventEnvelopeId Id { get; private set; }
    public EventEnvelopeMetadata Metadata { get; private set; }
    public EventEnvelopeStatus Status { get; private set; }
    public int Version { get; private set; }

    private EventEnvelopeAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static EventEnvelopeAggregate Seal(
        EventEnvelopeId id,
        EventEnvelopeMetadata metadata)
    {
        var aggregate = new EventEnvelopeAggregate();

        var @event = new EventEnvelopeSealedEvent(id, metadata);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Publish ─────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new EventEnvelopePublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Acknowledge ──────────────────────────────────────────────

    public void Acknowledge()
    {
        var specification = new CanAcknowledgeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, nameof(Acknowledge));

        var @event = new EventEnvelopeAcknowledgedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(EventEnvelopeSealedEvent @event)
    {
        Id = @event.EnvelopeId;
        Metadata = @event.Metadata;
        Status = EventEnvelopeStatus.Sealed;
        Version++;
    }

    private void Apply(EventEnvelopePublishedEvent @event)
    {
        Status = EventEnvelopeStatus.Published;
        Version++;
    }

    private void Apply(EventEnvelopeAcknowledgedEvent @event)
    {
        Status = EventEnvelopeStatus.Acknowledged;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EventEnvelopeErrors.MissingId();

        if (Metadata == default)
            throw EventEnvelopeErrors.MissingMetadata();

        if (!Enum.IsDefined(Status))
            throw EventEnvelopeErrors.InvalidStateTransition(Status, "validate");
    }
}
