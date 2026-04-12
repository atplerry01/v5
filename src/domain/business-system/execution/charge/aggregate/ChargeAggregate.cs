namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class ChargeAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<ChargeItem> _items = new();

    public ChargeId Id { get; private set; }
    public CostSourceId CostSourceId { get; private set; }
    public ChargeStatus Status { get; private set; }
    public IReadOnlyList<ChargeItem> Items => _items.AsReadOnly();
    public int Version { get; private set; }

    private ChargeAggregate() { }

    public static ChargeAggregate Create(ChargeId id, CostSourceId costSourceId)
    {
        var aggregate = new ChargeAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ChargeCreatedEvent(id, costSourceId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddItem(ChargeItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
    }

    public void Charge()
    {
        ValidateBeforeChange();

        var specification = new CanChargeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ChargeErrors.InvalidStateTransition(Status, nameof(Charge));

        if (_items.Count == 0)
            throw ChargeErrors.ItemRequired();

        var @event = new ChargeChargedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reverse()
    {
        ValidateBeforeChange();

        var specification = new CanReverseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ChargeErrors.InvalidStateTransition(Status, nameof(Reverse));

        var @event = new ChargeReversedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ChargeCreatedEvent @event)
    {
        Id = @event.ChargeId;
        CostSourceId = @event.CostSourceId;
        Status = ChargeStatus.Pending;
        Version++;
    }

    private void Apply(ChargeChargedEvent @event)
    {
        Status = ChargeStatus.Charged;
        Version++;
    }

    private void Apply(ChargeReversedEvent @event)
    {
        Status = ChargeStatus.Reversed;
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
            throw ChargeErrors.MissingId();

        if (CostSourceId == default)
            throw ChargeErrors.MissingCostSourceId();

        if (!Enum.IsDefined(Status))
            throw ChargeErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
