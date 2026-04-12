namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class EndpointAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EndpointId Id { get; private set; }
    public EndpointDefinition Definition { get; private set; } = null!;
    public EndpointStatus Status { get; private set; }
    public int Version { get; private set; }

    private EndpointAggregate() { }

    public static EndpointAggregate Create(EndpointId id, EndpointDefinition definition)
    {
        if (definition is null)
            throw EndpointErrors.MissingDefinition();

        var aggregate = new EndpointAggregate();
        aggregate.Definition = definition;
        aggregate.ValidateBeforeChange();

        var @event = new EndpointCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void MarkAvailable()
    {
        ValidateBeforeChange();

        var specification = new CanMarkAvailableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EndpointErrors.InvalidStateTransition(Status, nameof(MarkAvailable));

        var @event = new EndpointMarkedAvailableEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkUnavailable()
    {
        ValidateBeforeChange();

        var specification = new CanMarkUnavailableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EndpointErrors.InvalidStateTransition(Status, nameof(MarkUnavailable));

        var @event = new EndpointMarkedUnavailableEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EndpointCreatedEvent @event)
    {
        Id = @event.EndpointId;
        Status = EndpointStatus.Defined;
        Version++;
    }

    private void Apply(EndpointMarkedAvailableEvent @event)
    {
        Status = EndpointStatus.Available;
        Version++;
    }

    private void Apply(EndpointMarkedUnavailableEvent @event)
    {
        Status = EndpointStatus.Unavailable;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EndpointErrors.MissingId();

        if (Definition is null)
            throw EndpointErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw EndpointErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
