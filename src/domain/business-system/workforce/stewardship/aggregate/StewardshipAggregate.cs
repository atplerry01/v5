using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Stewardship;

public sealed class StewardshipAggregate : AggregateRoot
{
    public StewardshipId Id { get; private set; }
    public StewardshipDescriptor Descriptor { get; private set; }

    public static StewardshipAggregate Create(StewardshipId id, StewardshipDescriptor descriptor)
    {
        var aggregate = new StewardshipAggregate();
        if (aggregate.Version >= 0)
            throw StewardshipErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new StewardshipCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StewardshipCreatedEvent e:
                Id = e.StewardshipId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw StewardshipErrors.MissingId();

        if (Descriptor == default)
            throw StewardshipErrors.MissingDescriptor();
    }
}
