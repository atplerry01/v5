namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public sealed class ContractAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<ContractParty> _parties = new();

    public ContractId Id { get; private set; }
    public ContractStatus Status { get; private set; }
    public IReadOnlyList<ContractParty> Parties => _parties.AsReadOnly();
    public int Version { get; private set; }

    private ContractAggregate() { }

    public static ContractAggregate Create(ContractId id)
    {
        var aggregate = new ContractAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ContractCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddParty(ContractParty party)
    {
        if (party is null)
            throw new ArgumentNullException(nameof(party));

        _parties.Add(party);
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Activate));

        if (_parties.Count == 0)
            throw ContractErrors.PartyRequired();

        var @event = new ContractActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ContractSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new ContractTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ContractCreatedEvent @event)
    {
        Id = @event.ContractId;
        Status = ContractStatus.Draft;
        Version++;
    }

    private void Apply(ContractActivatedEvent @event)
    {
        Status = ContractStatus.Active;
        Version++;
    }

    private void Apply(ContractSuspendedEvent @event)
    {
        Status = ContractStatus.Suspended;
        Version++;
    }

    private void Apply(ContractTerminatedEvent @event)
    {
        Status = ContractStatus.Terminated;
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
            throw ContractErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ContractErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
