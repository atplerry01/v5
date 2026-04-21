using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class LifecycleAggregate : AggregateRoot
{
    public LifecycleId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public LifecycleStage Stage { get; private set; }
    public LifecycleStatus Status { get; private set; }

    public static LifecycleAggregate Start(
        LifecycleId id,
        CustomerRef customer,
        LifecycleStage initialStage,
        DateTimeOffset startedAt)
    {
        var aggregate = new LifecycleAggregate();
        if (aggregate.Version >= 0)
            throw LifecycleErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new LifecycleStartedEvent(id, customer, initialStage, startedAt));
        return aggregate;
    }

    public void ChangeStage(LifecycleStage to, DateTimeOffset changedAt)
    {
        var specification = new CanChangeStageSpecification();
        if (!specification.IsSatisfiedBy(Status, Stage, to))
        {
            if (Status != LifecycleStatus.Tracking)
                throw LifecycleErrors.ClosedImmutable(Id);
            throw LifecycleErrors.InvalidStageTransition(Stage, to);
        }

        RaiseDomainEvent(new LifecycleStageChangedEvent(Id, Stage, to, changedAt));
    }

    public void Close(DateTimeOffset closedAt)
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.ClosedImmutable(Id);

        RaiseDomainEvent(new LifecycleClosedEvent(Id, closedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LifecycleStartedEvent e:
                Id = e.LifecycleId;
                Customer = e.Customer;
                Stage = e.InitialStage;
                Status = LifecycleStatus.Tracking;
                break;
            case LifecycleStageChangedEvent e:
                Stage = e.To;
                break;
            case LifecycleClosedEvent:
                Status = LifecycleStatus.Closed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw LifecycleErrors.MissingId();

        if (Customer == default)
            throw LifecycleErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Stage))
            throw LifecycleErrors.InvalidStageTransition(Stage, Stage);

        if (!Enum.IsDefined(Status))
            throw LifecycleErrors.ClosedImmutable(Id);
    }
}
