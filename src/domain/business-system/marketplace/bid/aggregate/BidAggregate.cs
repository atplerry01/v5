namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public sealed class BidAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public BidId Id { get; private set; }
    public BidStatus Status { get; private set; }
    public BidReference Reference { get; private set; }
    public int Version { get; private set; }

    private BidAggregate() { }

    public static BidAggregate Create(BidId id, BidReference reference)
    {
        var aggregate = new BidAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new BidCreatedEvent(id, reference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Place()
    {
        ValidateBeforeChange();

        var specification = new CanPlaceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BidErrors.InvalidStateTransition(Status, nameof(Place));

        var @event = new BidPlacedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Withdraw()
    {
        ValidateBeforeChange();

        var specification = new CanWithdrawSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BidErrors.InvalidStateTransition(Status, nameof(Withdraw));

        var @event = new BidWithdrawnEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BidCreatedEvent @event)
    {
        Id = @event.BidId;
        Reference = @event.Reference;
        Status = BidStatus.Draft;
        Version++;
    }

    private void Apply(BidPlacedEvent @event)
    {
        Status = BidStatus.Placed;
        Version++;
    }

    private void Apply(BidWithdrawnEvent @event)
    {
        Status = BidStatus.Withdrawn;
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
            throw BidErrors.MissingId();

        if (Reference == default)
            throw BidErrors.MissingReference();

        if (!Enum.IsDefined(Status))
            throw BidErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
