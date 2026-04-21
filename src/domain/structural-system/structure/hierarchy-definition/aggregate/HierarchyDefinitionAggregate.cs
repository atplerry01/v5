using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public sealed class HierarchyDefinitionAggregate : AggregateRoot
{
    public HierarchyDefinitionId Id { get; private set; }
    public HierarchyDefinitionDescriptor Descriptor { get; private set; }
    public HierarchyDefinitionStatus Status { get; private set; }

    public static HierarchyDefinitionAggregate Define(HierarchyDefinitionId id, HierarchyDefinitionDescriptor descriptor)
    {
        var aggregate = new HierarchyDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw HierarchyDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new HierarchyDefinitionDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Validate()
    {
        var specification = new CanValidateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, nameof(Validate));

        RaiseDomainEvent(new HierarchyDefinitionValidatedEvent(Id));
    }

    public void Lock()
    {
        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, nameof(Lock));

        RaiseDomainEvent(new HierarchyDefinitionLockedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case HierarchyDefinitionDefinedEvent e:
                Id = e.HierarchyDefinitionId;
                Descriptor = e.Descriptor;
                Status = HierarchyDefinitionStatus.Defined;
                break;
            case HierarchyDefinitionValidatedEvent:
                Status = HierarchyDefinitionStatus.Validated;
                break;
            case HierarchyDefinitionLockedEvent:
                Status = HierarchyDefinitionStatus.Locked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw HierarchyDefinitionErrors.MissingId();

        if (Descriptor == default)
            throw HierarchyDefinitionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, "validate");

        if (Descriptor.ParentReference != Guid.Empty && Descriptor.ParentReference == Id.Value)
            throw HierarchyDefinitionErrors.InvalidParentChild();
    }
}
