namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed class CapacityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CapacityId Id { get; private set; }
    public CapacityStatus Status { get; private set; }
    public CapacityLimit Limit { get; private set; }
    public int Version { get; private set; }

    private CapacityAggregate() { }

    public static CapacityAggregate Create(CapacityId id, CapacityLimit limit)
    {
        var aggregate = new CapacityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CapacityCreatedEvent(id, limit);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateCapacitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CapacityErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CapacityActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendCapacitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CapacityErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new CapacitySuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reinstate()
    {
        ValidateBeforeChange();

        var specification = new CanReinstateCapacitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CapacityErrors.InvalidStateTransition(Status, nameof(Reinstate));

        var @event = new CapacityReinstatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CapacityCreatedEvent @event)
    {
        Id = @event.CapacityId;
        Limit = @event.Limit;
        Status = CapacityStatus.Pending;
        Version++;
    }

    private void Apply(CapacityActivatedEvent @event)
    {
        Status = CapacityStatus.Active;
        Version++;
    }

    private void Apply(CapacitySuspendedEvent @event)
    {
        Status = CapacityStatus.Suspended;
        Version++;
    }

    private void Apply(CapacityReinstatedEvent @event)
    {
        Status = CapacityStatus.Active;
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
            throw CapacityErrors.MissingId();

        if (Limit == default)
            throw CapacityErrors.MissingLimit();

        if (!Enum.IsDefined(Status))
            throw CapacityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
