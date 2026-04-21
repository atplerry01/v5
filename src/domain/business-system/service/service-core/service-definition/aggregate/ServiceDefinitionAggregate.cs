using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed class ServiceDefinitionAggregate : AggregateRoot
{
    public ServiceDefinitionId Id { get; private set; }
    public ServiceDefinitionName Name { get; private set; }
    public ServiceCategory Category { get; private set; }
    public ServiceDefinitionStatus Status { get; private set; }

    public static ServiceDefinitionAggregate Create(
        ServiceDefinitionId id,
        ServiceDefinitionName name,
        ServiceCategory category)
    {
        var aggregate = new ServiceDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw ServiceDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceDefinitionCreatedEvent(id, name, category));
        return aggregate;
    }

    public void Update(ServiceDefinitionName name, ServiceCategory category)
    {
        EnsureMutable();

        RaiseDomainEvent(new ServiceDefinitionUpdatedEvent(Id, name, category));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceDefinitionActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ServiceDefinitionStatus.Archived)
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceDefinitionArchivedEvent(Id));
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceDefinitionErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceDefinitionCreatedEvent e:
                Id = e.ServiceDefinitionId;
                Name = e.Name;
                Category = e.Category;
                Status = ServiceDefinitionStatus.Draft;
                break;
            case ServiceDefinitionUpdatedEvent e:
                Name = e.Name;
                Category = e.Category;
                break;
            case ServiceDefinitionActivatedEvent:
                Status = ServiceDefinitionStatus.Active;
                break;
            case ServiceDefinitionArchivedEvent:
                Status = ServiceDefinitionStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceDefinitionErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
