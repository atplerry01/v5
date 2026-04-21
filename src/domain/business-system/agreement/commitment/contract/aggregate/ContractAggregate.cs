using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public sealed class ContractAggregate : AggregateRoot
{
    private readonly List<ContractParty> _parties = new();

    public ContractId Id { get; private set; }
    public ContractStatus Status { get; private set; }
    public IReadOnlyList<ContractParty> Parties => _parties.AsReadOnly();

    public static ContractAggregate Create(ContractId id, DateTimeOffset createdAt)
    {
        var aggregate = new ContractAggregate();
        if (aggregate.Version >= 0)
            throw ContractErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ContractCreatedEvent(id, createdAt));
        return aggregate;
    }

    public void AddParty(PartyId partyId, string role)
    {
        Guard.Against(partyId == default, "PartyId must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(role), "Role must not be empty.");

        RaiseDomainEvent(new ContractPartyAddedEvent(Id, partyId, role));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Activate));

        if (_parties.Count == 0)
            throw ContractErrors.PartyRequired();

        RaiseDomainEvent(new ContractActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new ContractSuspendedEvent(Id));
    }

    public void Terminate()
    {
        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Terminate));

        RaiseDomainEvent(new ContractTerminatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ContractCreatedEvent e:
                Id = e.ContractId;
                Status = ContractStatus.Draft;
                break;
            case ContractPartyAddedEvent e:
                _parties.Add(new ContractParty(e.PartyId, e.Role));
                break;
            case ContractActivatedEvent:
                Status = ContractStatus.Active;
                break;
            case ContractSuspendedEvent:
                Status = ContractStatus.Suspended;
                break;
            case ContractTerminatedEvent:
                Status = ContractStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ContractErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ContractErrors.InvalidStateTransition(Status, "validate");
    }
}
