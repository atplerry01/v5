namespace Whycespace.Domain.BusinessSystem.Marketplace.SettlementMarket;

public sealed class SettlementMarketAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SettlementMarketId Id { get; private set; }
    public SettlementMarketStatus Status { get; private set; }
    public SettlementTerms Terms { get; private set; }
    public int Version { get; private set; }

    private SettlementMarketAggregate() { }

    public static SettlementMarketAggregate Create(SettlementMarketId id)
    {
        var aggregate = new SettlementMarketAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SettlementMarketCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Define(SettlementTerms terms)
    {
        ValidateBeforeChange();

        var specification = new CanDefineSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SettlementMarketErrors.InvalidStateTransition(Status, nameof(Define));

        var @event = new SettlementMarketDefinedEvent(Id, terms);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Seal()
    {
        ValidateBeforeChange();

        var specification = new CanSealSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SettlementMarketErrors.InvalidStateTransition(Status, nameof(Seal));

        if (Terms == default)
            throw SettlementMarketErrors.MissingTerms();

        var @event = new SettlementMarketSealedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SettlementMarketCreatedEvent @event)
    {
        Id = @event.SettlementMarketId;
        Status = SettlementMarketStatus.Draft;
        Version++;
    }

    private void Apply(SettlementMarketDefinedEvent @event)
    {
        Terms = @event.Terms;
        Status = SettlementMarketStatus.Defined;
        Version++;
    }

    private void Apply(SettlementMarketSealedEvent @event)
    {
        Status = SettlementMarketStatus.Sealed;
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
            throw SettlementMarketErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SettlementMarketErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
