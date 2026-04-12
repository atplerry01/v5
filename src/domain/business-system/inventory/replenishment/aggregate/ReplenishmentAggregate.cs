namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed class ReplenishmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReplenishmentId Id { get; private set; }
    public ReplenishmentThreshold Threshold { get; private set; }
    public RestockQuantity RestockQuantity { get; private set; }
    public ReplenishmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private ReplenishmentAggregate() { }

    public static ReplenishmentAggregate Create(
        ReplenishmentId id,
        ReplenishmentThreshold threshold,
        RestockQuantity restockQuantity)
    {
        var aggregate = new ReplenishmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ReplenishmentCreatedEvent(id, threshold, restockQuantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendReplenishmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReplenishmentErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ReplenishmentSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateReplenishmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReplenishmentErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new ReplenishmentReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateReplenishmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReplenishmentErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ReplenishmentDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReplenishmentCreatedEvent @event)
    {
        Id = @event.ReplenishmentId;
        Threshold = @event.Threshold;
        RestockQuantity = @event.RestockQuantity;
        Status = ReplenishmentStatus.Active;
        Version++;
    }

    private void Apply(ReplenishmentSuspendedEvent @event)
    {
        Status = ReplenishmentStatus.Suspended;
        Version++;
    }

    private void Apply(ReplenishmentReactivatedEvent @event)
    {
        Status = ReplenishmentStatus.Active;
        Version++;
    }

    private void Apply(ReplenishmentDeactivatedEvent @event)
    {
        Status = ReplenishmentStatus.Deactivated;
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
            throw ReplenishmentErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ReplenishmentErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
