namespace Whycespace.Domain.BusinessSystem.Entitlement.Right;

public sealed class RightAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RightId Id { get; private set; }
    public RightDefinition Definition { get; private set; }
    public RightStatus Status { get; private set; }
    public int Version { get; private set; }

    private RightAggregate() { }

    public static RightAggregate Create(RightId id, RightDefinition definition)
    {
        if (definition is null)
            throw new ArgumentNullException(nameof(definition));

        var aggregate = new RightAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RightCreatedEvent(id, definition.ScopeId, definition.Capability, definition.Constraints);
        aggregate.Apply(@event, definition);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RightErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new RightActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        ValidateBeforeChange();

        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RightErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new RightDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RightCreatedEvent @event, RightDefinition definition)
    {
        Id = @event.RightId;
        Definition = definition;
        Status = RightStatus.Defined;
        Version++;
    }

    private void Apply(RightActivatedEvent @event)
    {
        Status = RightStatus.Active;
        Version++;
    }

    private void Apply(RightDeprecatedEvent @event)
    {
        Status = RightStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RightErrors.MissingId();

        if (Definition is null)
            throw RightErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw RightErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
