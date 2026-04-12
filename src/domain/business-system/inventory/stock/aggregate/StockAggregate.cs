namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed class StockAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public StockId Id { get; private set; }
    public StockItemId ItemId { get; private set; }
    public StockStatus Status { get; private set; }
    public Quantity CurrentQuantity { get; private set; }
    public int Version { get; private set; }

    private StockAggregate() { }

    public static StockAggregate Create(StockId id, StockItemId itemId, int initialQuantity)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity must not be negative.", nameof(initialQuantity));

        var aggregate = new StockAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new StockCreatedEvent(id, itemId, initialQuantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Track()
    {
        ValidateBeforeChange();

        var specification = new CanTrackSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StockErrors.InvalidStateTransition(Status, nameof(Track));

        var @event = new StockTrackedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deplete()
    {
        ValidateBeforeChange();

        var specification = new CanDepleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StockErrors.InvalidStateTransition(Status, nameof(Deplete));

        var @event = new StockDepletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(StockCreatedEvent @event)
    {
        Id = @event.StockId;
        ItemId = @event.ItemId;
        CurrentQuantity = new Quantity(@event.InitialQuantity);
        Status = StockStatus.Initialized;
        Version++;
    }

    private void Apply(StockTrackedEvent @event)
    {
        Status = StockStatus.Tracked;
        Version++;
    }

    private void Apply(StockDepletedEvent @event)
    {
        Status = StockStatus.Depleted;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw StockErrors.MissingId();

        if (ItemId == default)
            throw StockErrors.MissingItemId();

        if (CurrentQuantity.Value < 0)
            throw StockErrors.NegativeQuantity();

        if (!Enum.IsDefined(Status))
            throw StockErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
