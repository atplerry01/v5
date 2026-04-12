namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public sealed class WarehouseAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public WarehouseId Id { get; private set; }
    public WarehouseCapacity Capacity { get; private set; }
    public WarehouseStatus Status { get; private set; }
    public int Version { get; private set; }

    private WarehouseAggregate() { }

    public static WarehouseAggregate Create(WarehouseId id, WarehouseCapacity capacity)
    {
        var aggregate = new WarehouseAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new WarehouseCreatedEvent(id, capacity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateWarehouseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw WarehouseErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new WarehouseDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(WarehouseCreatedEvent @event)
    {
        Id = @event.WarehouseId;
        Capacity = @event.Capacity;
        Status = WarehouseStatus.Active;
        Version++;
    }

    private void Apply(WarehouseDeactivatedEvent @event)
    {
        Status = WarehouseStatus.Deactivated;
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
            throw WarehouseErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw WarehouseErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
