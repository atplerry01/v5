namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed class CounterpartyAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CounterpartyId Id { get; private set; }
    public CounterpartyStatus Status { get; private set; }
    public CounterpartyProfile Profile { get; private set; } = null!;
    public int Version { get; private set; }

    private CounterpartyAggregate() { }

    public static CounterpartyAggregate Create(CounterpartyId id, CounterpartyProfile profile)
    {
        if (profile is null)
            throw CounterpartyErrors.MissingProfile();

        var aggregate = new CounterpartyAggregate();
        aggregate.Profile = profile;
        aggregate.ValidateBeforeChange();

        var @event = new CounterpartyCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new CounterpartySuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new CounterpartyTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CounterpartyCreatedEvent @event)
    {
        Id = @event.CounterpartyId;
        Status = CounterpartyStatus.Active;
        Version++;
    }

    private void Apply(CounterpartySuspendedEvent @event)
    {
        Status = CounterpartyStatus.Suspended;
        Version++;
    }

    private void Apply(CounterpartyTerminatedEvent @event)
    {
        Status = CounterpartyStatus.Terminated;
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
            throw CounterpartyErrors.MissingId();

        if (Profile is null)
            throw CounterpartyErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
