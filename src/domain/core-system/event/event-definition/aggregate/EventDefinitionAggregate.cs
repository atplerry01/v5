namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed class EventDefinitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EventDefinitionId Id { get; private set; }
    public EventSchema Schema { get; private set; }
    public EventDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private EventDefinitionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static EventDefinitionAggregate Register(
        EventDefinitionId id,
        EventSchema schema)
    {
        var aggregate = new EventDefinitionAggregate();

        var @event = new EventDefinitionRegisteredEvent(id, schema);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Publish ──────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new EventDefinitionPublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new EventDefinitionDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(EventDefinitionRegisteredEvent @event)
    {
        Id = @event.DefinitionId;
        Schema = @event.Schema;
        Status = EventDefinitionStatus.Draft;
        Version++;
    }

    private void Apply(EventDefinitionPublishedEvent @event)
    {
        Status = EventDefinitionStatus.Published;
        Version++;
    }

    private void Apply(EventDefinitionDeprecatedEvent @event)
    {
        Status = EventDefinitionStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EventDefinitionErrors.MissingId();

        if (Schema == default)
            throw EventDefinitionErrors.MissingSchema();

        if (!Enum.IsDefined(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
