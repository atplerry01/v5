namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed class HoldingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public HoldingId Id { get; private set; }
    public PortfolioReference PortfolioReference { get; private set; }
    public AssetReference AssetReference { get; private set; }
    public HoldingQuantity Quantity { get; private set; }
    public HoldingStatus Status { get; private set; }
    public int Version { get; private set; }

    private HoldingAggregate() { }

    public static HoldingAggregate Create(
        HoldingId id,
        PortfolioReference portfolioReference,
        AssetReference assetReference,
        HoldingQuantity quantity)
    {
        var aggregate = new HoldingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new HoldingCreatedEvent(id, portfolioReference, assetReference, quantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateHoldingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HoldingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new HoldingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HoldingErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new HoldingSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseHoldingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HoldingErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new HoldingClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(HoldingCreatedEvent @event)
    {
        Id = @event.HoldingId;
        PortfolioReference = @event.PortfolioReference;
        AssetReference = @event.AssetReference;
        Quantity = @event.Quantity;
        Status = HoldingStatus.Opened;
        Version++;
    }

    private void Apply(HoldingActivatedEvent @event)
    {
        Status = HoldingStatus.Active;
        Version++;
    }

    private void Apply(HoldingSuspendedEvent @event)
    {
        Status = HoldingStatus.Suspended;
        Version++;
    }

    private void Apply(HoldingClosedEvent @event)
    {
        Status = HoldingStatus.Closed;
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
            throw HoldingErrors.MissingId();

        if (PortfolioReference == default)
            throw HoldingErrors.PortfolioReferenceRequired();

        if (AssetReference == default)
            throw HoldingErrors.AssetReferenceRequired();

        if (Quantity == default)
            throw HoldingErrors.QuantityMustBePositive();

        if (!Enum.IsDefined(Status))
            throw HoldingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
