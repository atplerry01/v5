namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed class EventBridgeAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EventBridgeId Id { get; private set; }
    public EventMappingId MappingId { get; private set; }
    public EventBridgeStatus Status { get; private set; }
    public int Version { get; private set; }

    private EventBridgeAggregate() { }

    public static EventBridgeAggregate Create(EventBridgeId id, EventMappingId mappingId)
    {
        var aggregate = new EventBridgeAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EventBridgeCreatedEvent(id, mappingId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventBridgeErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new EventBridgeActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventBridgeErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new EventBridgeDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EventBridgeCreatedEvent @event)
    {
        Id = @event.EventBridgeId;
        MappingId = @event.MappingId;
        Status = EventBridgeStatus.Defined;
        Version++;
    }

    private void Apply(EventBridgeActivatedEvent @event)
    {
        Status = EventBridgeStatus.Active;
        Version++;
    }

    private void Apply(EventBridgeDisabledEvent @event)
    {
        Status = EventBridgeStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EventBridgeErrors.MissingId();

        if (MappingId == default)
            throw EventBridgeErrors.MissingMappingId();

        if (!Enum.IsDefined(Status))
            throw EventBridgeErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
