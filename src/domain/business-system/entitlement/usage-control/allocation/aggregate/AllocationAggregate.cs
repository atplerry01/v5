using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed class AllocationAggregate : AggregateRoot
{
    public AllocationId Id { get; private set; }
    public ResourceId ResourceId { get; private set; }
    public AllocationStatus Status { get; private set; }
    public int RequestedCapacity { get; private set; }

    public static AllocationAggregate Create(AllocationId id, ResourceId resourceId, int requestedCapacity)
    {
        Guard.Against(requestedCapacity <= 0, "Requested capacity must be greater than zero.");

        var aggregate = new AllocationAggregate();
        if (aggregate.Version >= 0)
            throw AllocationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AllocationCreatedEvent(id, resourceId, requestedCapacity));
        return aggregate;
    }

    public void Allocate()
    {
        var specification = new CanAllocateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Allocate));

        RaiseDomainEvent(new AllocationAllocatedEvent(Id));
    }

    public void Release()
    {
        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Release));

        RaiseDomainEvent(new AllocationReleasedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AllocationCreatedEvent e:
                Id = e.AllocationId;
                ResourceId = e.ResourceId;
                RequestedCapacity = e.RequestedCapacity;
                Status = AllocationStatus.Pending;
                break;
            case AllocationAllocatedEvent:
                Status = AllocationStatus.Allocated;
                break;
            case AllocationReleasedEvent:
                Status = AllocationStatus.Released;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AllocationErrors.MissingId();

        if (ResourceId == default)
            throw AllocationErrors.MissingResourceId();

        if (!Enum.IsDefined(Status))
            throw AllocationErrors.InvalidStateTransition(Status, "validate");
    }
}
