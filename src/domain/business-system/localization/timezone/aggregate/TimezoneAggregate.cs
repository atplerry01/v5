namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public sealed class TimezoneAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TimezoneId Id { get; private set; }
    public TimezoneStatus Status { get; private set; }
    public TimezoneOffset Offset { get; private set; }
    public int Version { get; private set; }

    private TimezoneAggregate() { }

    public static TimezoneAggregate Create(TimezoneId id, TimezoneOffset offset)
    {
        var aggregate = new TimezoneAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TimezoneCreatedEvent(id, offset);
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
            throw TimezoneErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TimezoneActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TimezoneErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new TimezoneDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TimezoneCreatedEvent @event)
    {
        Id = @event.TimezoneId;
        Offset = @event.Offset;
        Status = TimezoneStatus.Draft;
        Version++;
    }

    private void Apply(TimezoneActivatedEvent @event)
    {
        Status = TimezoneStatus.Active;
        Version++;
    }

    private void Apply(TimezoneDeactivatedEvent @event)
    {
        Status = TimezoneStatus.Deactivated;
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
            throw TimezoneErrors.MissingId();

        if (Offset == default)
            throw TimezoneErrors.InvalidTimezoneOffset();

        if (!Enum.IsDefined(Status))
            throw TimezoneErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
