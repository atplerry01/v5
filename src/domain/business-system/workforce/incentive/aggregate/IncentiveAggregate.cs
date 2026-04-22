using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Incentive;

public sealed class IncentiveAggregate : AggregateRoot
{
    public IncentiveId Id { get; private set; }
    public IncentiveDescriptor Descriptor { get; private set; }

    public static IncentiveAggregate Create(IncentiveId id, IncentiveDescriptor descriptor)
    {
        var aggregate = new IncentiveAggregate();
        if (aggregate.Version >= 0)
            throw IncentiveErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new IncentiveCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IncentiveCreatedEvent e:
                Id = e.IncentiveId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw IncentiveErrors.MissingId();

        if (Descriptor == default)
            throw IncentiveErrors.MissingDescriptor();
    }
}
