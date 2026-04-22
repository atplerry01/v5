using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.HumancapitalSanction;

public sealed class SanctionAggregate : AggregateRoot
{
    public SanctionId Id { get; private set; }
    public SanctionDescriptor Descriptor { get; private set; }

    public static SanctionAggregate Create(SanctionId id, SanctionDescriptor descriptor)
    {
        var aggregate = new SanctionAggregate();
        if (aggregate.Version >= 0)
            throw SanctionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SanctionCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SanctionCreatedEvent e:
                Id = e.SanctionId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SanctionErrors.MissingId();

        if (Descriptor == default)
            throw SanctionErrors.MissingDescriptor();
    }
}
