namespace Whycespace.Domain.BusinessSystem.Marketplace.ParticipantMarket;

public sealed class ParticipantMarketAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ParticipantMarketId Id { get; private set; }
    public ParticipantMarketStatus Status { get; private set; }
    public ParticipantReference Reference { get; private set; }
    public int Version { get; private set; }

    private ParticipantMarketAggregate() { }

    public static ParticipantMarketAggregate Create(ParticipantMarketId id, ParticipantReference reference)
    {
        var aggregate = new ParticipantMarketAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ParticipantMarketRegisteredEvent(id, reference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ParticipantMarketErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ParticipantMarketActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ParticipantMarketErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ParticipantMarketSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ParticipantMarketRegisteredEvent @event)
    {
        Id = @event.ParticipantMarketId;
        Reference = @event.Reference;
        Status = ParticipantMarketStatus.Registered;
        Version++;
    }

    private void Apply(ParticipantMarketActivatedEvent @event)
    {
        Status = ParticipantMarketStatus.Active;
        Version++;
    }

    private void Apply(ParticipantMarketSuspendedEvent @event)
    {
        Status = ParticipantMarketStatus.Suspended;
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
            throw ParticipantMarketErrors.MissingId();

        if (Reference == default)
            throw ParticipantMarketErrors.MissingReference();

        if (!Enum.IsDefined(Status))
            throw ParticipantMarketErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
