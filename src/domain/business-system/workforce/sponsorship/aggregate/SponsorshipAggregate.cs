using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Sponsorship;

public sealed class SponsorshipAggregate : AggregateRoot
{
    public SponsorshipId Id { get; private set; }
    public SponsorshipDescriptor Descriptor { get; private set; }

    public static SponsorshipAggregate Create(SponsorshipId id, SponsorshipDescriptor descriptor)
    {
        var aggregate = new SponsorshipAggregate();
        if (aggregate.Version >= 0)
            throw SponsorshipErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SponsorshipCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SponsorshipCreatedEvent e:
                Id = e.SponsorshipId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SponsorshipErrors.MissingId();

        if (Descriptor == default)
            throw SponsorshipErrors.MissingDescriptor();
    }
}
