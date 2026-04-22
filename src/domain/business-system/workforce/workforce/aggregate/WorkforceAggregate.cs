using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Workforce;

public sealed class WorkforceAggregate : AggregateRoot
{
    public WorkforceId Id { get; private set; }
    public WorkforceDescriptor Descriptor { get; private set; }

    public static WorkforceAggregate Create(WorkforceId id, WorkforceDescriptor descriptor)
    {
        var aggregate = new WorkforceAggregate();
        if (aggregate.Version >= 0)
            throw WorkforceErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new WorkforceCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case WorkforceCreatedEvent e:
                Id = e.WorkforceId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw WorkforceErrors.MissingId();

        if (Descriptor == default)
            throw WorkforceErrors.MissingDescriptor();
    }
}
