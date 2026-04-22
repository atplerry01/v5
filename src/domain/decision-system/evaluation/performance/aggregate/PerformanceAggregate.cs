using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Performance;

public sealed class PerformanceAggregate : AggregateRoot
{
    public PerformanceId Id { get; private set; }
    public PerformanceDescriptor Descriptor { get; private set; }

    public static PerformanceAggregate Create(PerformanceId id, PerformanceDescriptor descriptor)
    {
        var aggregate = new PerformanceAggregate();
        if (aggregate.Version >= 0)
            throw PerformanceErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new PerformanceCreatedEvent(id, descriptor));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PerformanceCreatedEvent e:
                Id = e.PerformanceId;
                Descriptor = e.Descriptor;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw PerformanceErrors.MissingId();

        if (Descriptor == default)
            throw PerformanceErrors.MissingDescriptor();
    }
}
