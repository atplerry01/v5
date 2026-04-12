using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public sealed class InstanceAggregate : AggregateRoot
{
    public InstanceId InstanceId { get; private set; }
    public InstanceContext Context { get; private set; }
    public InstanceStatus Status { get; private set; }

    public static InstanceAggregate Create(InstanceId id, InstanceContext context)
    {
        var aggregate = new InstanceAggregate();
        aggregate.RaiseDomainEvent(new InstanceCreatedEvent(id, context));
        return aggregate;
    }

    public void Start()
    {
        if (!new CanStartSpecification().IsSatisfiedBy(this))
            throw InstanceErrors.InvalidStateTransition(Status, "Start");

        RaiseDomainEvent(new InstanceStartedEvent(InstanceId));
    }

    public void Complete()
    {
        if (!new CanCompleteSpecification().IsSatisfiedBy(this))
            throw InstanceErrors.InvalidStateTransition(Status, "Complete");

        RaiseDomainEvent(new InstanceCompletedEvent(InstanceId));
    }

    public void Fail()
    {
        if (!new CanFailSpecification().IsSatisfiedBy(this))
            throw InstanceErrors.InvalidStateTransition(Status, "Fail");

        RaiseDomainEvent(new InstanceFailedEvent(InstanceId));
    }

    public void Terminate()
    {
        if (!new CanTerminateSpecification().IsSatisfiedBy(this))
            throw InstanceErrors.InvalidStateTransition(Status, "Terminate");

        RaiseDomainEvent(new InstanceTerminatedEvent(InstanceId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case InstanceCreatedEvent e:
                InstanceId = e.InstanceId;
                Context = e.Context;
                Status = InstanceStatus.Created;
                break;

            case InstanceStartedEvent:
                Status = InstanceStatus.Running;
                break;

            case InstanceCompletedEvent:
                Status = InstanceStatus.Completed;
                break;

            case InstanceFailedEvent:
                Status = InstanceStatus.Failed;
                break;

            case InstanceTerminatedEvent:
                Status = InstanceStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        // InstanceId and Context are validated at construction via their value object constructors.
        // Status transitions are enforced by specifications before events are raised.
    }
}
