using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed class ServiceOfferingAggregate : AggregateRoot
{
    public ServiceOfferingId Id { get; private set; }
    public ServiceOfferingName Name { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public ServiceOfferingStatus Status { get; private set; }
    public OfferingPackageRef? Package { get; private set; }

    public static ServiceOfferingAggregate Create(
        ServiceOfferingId id,
        ServiceOfferingName name,
        ServiceDefinitionRef serviceDefinition,
        OfferingPackageRef? package = null)
    {
        var aggregate = new ServiceOfferingAggregate();
        if (aggregate.Version >= 0)
            throw ServiceOfferingErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceOfferingCreatedEvent(id, name, serviceDefinition, package));
        return aggregate;
    }

    public void Update(ServiceOfferingName name)
    {
        EnsureMutable(nameof(Update));

        RaiseDomainEvent(new ServiceOfferingUpdatedEvent(Id, name));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOfferingErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceOfferingActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ServiceOfferingStatus.Archived)
            throw ServiceOfferingErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceOfferingArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceOfferingCreatedEvent e:
                Id = e.ServiceOfferingId;
                Name = e.Name;
                ServiceDefinition = e.ServiceDefinition;
                Package = e.Package;
                Status = ServiceOfferingStatus.Draft;
                break;
            case ServiceOfferingUpdatedEvent e:
                Name = e.Name;
                break;
            case ServiceOfferingActivatedEvent:
                Status = ServiceOfferingStatus.Active;
                break;
            case ServiceOfferingArchivedEvent:
                Status = ServiceOfferingStatus.Archived;
                break;
        }
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOfferingErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceOfferingErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceOfferingErrors.MissingServiceDefinition();

        if (!Enum.IsDefined(Status))
            throw ServiceOfferingErrors.InvalidStateTransition(Status, "validate");
    }
}
