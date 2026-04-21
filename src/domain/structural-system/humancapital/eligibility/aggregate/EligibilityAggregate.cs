using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

public sealed class EligibilityAggregate : AggregateRoot
{
    public EligibilityId Id { get; private set; }
    public EligibilityDescriptor Descriptor { get; private set; }

    public static EligibilityAggregate Create(EligibilityId id, EligibilityDescriptor descriptor)
    {
        var aggregate = new EligibilityAggregate();
        if (aggregate.Version >= 0)
            throw EligibilityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EligibilityCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EligibilityCreatedEvent e:
                Id = e.EligibilityId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EligibilityErrors.MissingId();

        if (Descriptor == default)
            throw EligibilityErrors.MissingDescriptor();
    }
}
