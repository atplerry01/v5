using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public sealed class TopologyDefinitionAggregate : AggregateRoot
{
    public TopologyDefinitionId Id { get; private set; }
    public TopologyDefinitionDescriptor Descriptor { get; private set; }
    public TopologyDefinitionStatus Status { get; private set; }

    public static TopologyDefinitionAggregate Create(TopologyDefinitionId id, TopologyDefinitionDescriptor descriptor)
    {
        var aggregate = new TopologyDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw TopologyDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new TopologyDefinitionCreatedEvent(id, descriptor));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyDefinitionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new TopologyDefinitionActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyDefinitionErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new TopologyDefinitionSuspendedEvent(Id));
    }

    public void Reactivate()
    {
        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyDefinitionErrors.InvalidStateTransition(Status, nameof(Reactivate));

        RaiseDomainEvent(new TopologyDefinitionReactivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyDefinitionErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new TopologyDefinitionRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TopologyDefinitionCreatedEvent e:
                Id = e.TopologyDefinitionId;
                Descriptor = e.Descriptor;
                Status = TopologyDefinitionStatus.Draft;
                break;
            case TopologyDefinitionActivatedEvent:
                Status = TopologyDefinitionStatus.Active;
                break;
            case TopologyDefinitionSuspendedEvent:
                Status = TopologyDefinitionStatus.Suspended;
                break;
            case TopologyDefinitionReactivatedEvent:
                Status = TopologyDefinitionStatus.Active;
                break;
            case TopologyDefinitionRetiredEvent:
                Status = TopologyDefinitionStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw TopologyDefinitionErrors.MissingId();

        if (Descriptor == default)
            throw TopologyDefinitionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TopologyDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
