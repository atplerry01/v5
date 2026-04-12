namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public sealed class LotAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LotId Id { get; private set; }
    public LotOrigin Origin { get; private set; }
    public LotStatus Status { get; private set; }
    public int Version { get; private set; }

    private LotAggregate() { }

    public static LotAggregate Create(LotId id, LotOrigin origin)
    {
        var aggregate = new LotAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new LotCreatedEvent(id, origin);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Seal()
    {
        ValidateBeforeChange();

        var specification = new CanSealLotSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LotErrors.InvalidStateTransition(Status, nameof(Seal));

        var @event = new LotSealedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LotCreatedEvent @event)
    {
        Id = @event.LotId;
        Origin = @event.Origin;
        Status = LotStatus.Active;
        Version++;
    }

    private void Apply(LotSealedEvent @event)
    {
        Status = LotStatus.Sealed;
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
            throw LotErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw LotErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
