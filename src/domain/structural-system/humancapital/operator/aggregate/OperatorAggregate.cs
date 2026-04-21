using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public sealed class OperatorAggregate : AggregateRoot
{
    public OperatorId Id { get; private set; }
    public OperatorDescriptor Descriptor { get; private set; }

    public static OperatorAggregate Create(OperatorId id, OperatorDescriptor descriptor)
    {
        var aggregate = new OperatorAggregate();
        if (aggregate.Version >= 0)
            throw OperatorErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new OperatorCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case OperatorCreatedEvent e:
                Id = e.OperatorId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw OperatorErrors.MissingId();

        if (Descriptor == default)
            throw OperatorErrors.MissingDescriptor();
    }
}
