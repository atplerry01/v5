namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public sealed class ItemAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ItemId Id { get; private set; }
    public ItemTypeId TypeId { get; private set; }
    public string ItemName { get; private set; }
    public ItemStatus Status { get; private set; }
    public int Version { get; private set; }

    private ItemAggregate() { }

    public static ItemAggregate Create(ItemId id, ItemTypeId typeId, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name must not be empty.", nameof(itemName));

        var aggregate = new ItemAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ItemCreatedEvent(id, typeId, itemName);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Discontinue()
    {
        ValidateBeforeChange();

        var specification = new CanDiscontinueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ItemErrors.InvalidStateTransition(Status, nameof(Discontinue));

        var @event = new ItemDiscontinuedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ItemCreatedEvent @event)
    {
        Id = @event.ItemId;
        TypeId = @event.TypeId;
        ItemName = @event.ItemName;
        Status = ItemStatus.Active;
        Version++;
    }

    private void Apply(ItemDiscontinuedEvent @event)
    {
        Status = ItemStatus.Discontinued;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ItemErrors.MissingId();

        if (TypeId == default)
            throw ItemErrors.MissingTypeId();

        if (!Enum.IsDefined(Status))
            throw ItemErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
