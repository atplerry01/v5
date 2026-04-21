using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public sealed class CounterpartyAggregate : AggregateRoot
{
    public CounterpartyId Id { get; private set; }
    public CounterpartyStatus Status { get; private set; }
    public CounterpartyProfile Profile { get; private set; } = null!;

    public static CounterpartyAggregate Create(CounterpartyId id, CounterpartyProfile profile)
    {
        if (profile is null)
            throw CounterpartyErrors.MissingProfile();

        var aggregate = new CounterpartyAggregate();
        if (aggregate.Version >= 0)
            throw CounterpartyErrors.AlreadyInitialized();

        aggregate.Profile = profile;

        aggregate.RaiseDomainEvent(new CounterpartyCreatedEvent(id));
        return aggregate;
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new CounterpartySuspendedEvent(Id));
    }

    public void Terminate()
    {
        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, nameof(Terminate));

        RaiseDomainEvent(new CounterpartyTerminatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CounterpartyCreatedEvent e:
                Id = e.CounterpartyId;
                Status = CounterpartyStatus.Active;
                break;
            case CounterpartySuspendedEvent:
                Status = CounterpartyStatus.Suspended;
                break;
            case CounterpartyTerminatedEvent:
                Status = CounterpartyStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw CounterpartyErrors.MissingId();

        if (Profile is null)
            throw CounterpartyErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw CounterpartyErrors.InvalidStateTransition(Status, "validate");
    }
}
