namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class CallbackAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CallbackId Id { get; private set; }
    public CallbackDefinition Definition { get; private set; } = null!;
    public CallbackStatus Status { get; private set; }
    public int Version { get; private set; }

    private CallbackAggregate() { }

    public static CallbackAggregate Create(CallbackId id, CallbackDefinition definition)
    {
        if (definition is null)
            throw new ArgumentNullException(nameof(definition));

        var aggregate = new CallbackAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CallbackCreatedEvent(id, definition.DefinitionId, definition.CallbackName);
        aggregate.Apply(@event, definition);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void MarkPending()
    {
        ValidateBeforeChange();

        var specification = new CanMarkPendingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CallbackErrors.InvalidStateTransition(Status, nameof(MarkPending));

        var @event = new CallbackMarkedPendingEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CallbackErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new CallbackCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CallbackCreatedEvent @event, CallbackDefinition definition)
    {
        Id = @event.CallbackId;
        Definition = definition;
        Status = CallbackStatus.Registered;
        Version++;
    }

    private void Apply(CallbackMarkedPendingEvent @event)
    {
        Status = CallbackStatus.Pending;
        Version++;
    }

    private void Apply(CallbackCompletedEvent @event)
    {
        Status = CallbackStatus.Completed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CallbackErrors.MissingId();

        if (Definition is null)
            throw CallbackErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw CallbackErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
