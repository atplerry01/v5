using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Governance;

public sealed class GovernanceAggregate : AggregateRoot
{
    public GovernanceId Id { get; private set; }
    public GovernanceDescriptor Descriptor { get; private set; }

    public static GovernanceAggregate Create(GovernanceId id, GovernanceDescriptor descriptor)
    {
        var aggregate = new GovernanceAggregate();
        if (aggregate.Version >= 0)
            throw GovernanceErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new GovernanceCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case GovernanceCreatedEvent e:
                Id = e.GovernanceId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw GovernanceErrors.MissingId();

        if (Descriptor == default)
            throw GovernanceErrors.MissingDescriptor();
    }
}
