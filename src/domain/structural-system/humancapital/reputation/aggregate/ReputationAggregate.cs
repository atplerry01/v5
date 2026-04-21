using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Reputation;

public sealed class ReputationAggregate : AggregateRoot
{
    public ReputationId Id { get; private set; }
    public ReputationDescriptor Descriptor { get; private set; }

    public static ReputationAggregate Create(ReputationId id, ReputationDescriptor descriptor)
    {
        var aggregate = new ReputationAggregate();
        if (aggregate.Version >= 0)
            throw ReputationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ReputationCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReputationCreatedEvent e:
                Id = e.ReputationId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ReputationErrors.MissingId();

        if (Descriptor == default)
            throw ReputationErrors.MissingDescriptor();
    }
}
