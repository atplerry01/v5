namespace Whycespace.Domain.BusinessSystem.Inventory.Sku;

public sealed class SkuAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SkuId Id { get; private set; }
    public SkuCode Code { get; private set; }
    public SkuStatus Status { get; private set; }
    public int Version { get; private set; }

    private SkuAggregate() { }

    public static SkuAggregate Create(SkuId id, SkuCode code)
    {
        var aggregate = new SkuAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SkuCreatedEvent(id, code);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Discontinue()
    {
        ValidateBeforeChange();

        var specification = new CanDiscontinueSkuSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SkuErrors.InvalidStateTransition(Status, nameof(Discontinue));

        var @event = new SkuDiscontinuedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SkuCreatedEvent @event)
    {
        Id = @event.SkuId;
        Code = @event.Code;
        Status = SkuStatus.Active;
        Version++;
    }

    private void Apply(SkuDiscontinuedEvent @event)
    {
        Status = SkuStatus.Discontinued;
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
            throw SkuErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SkuErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
