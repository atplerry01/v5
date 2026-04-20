using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed class ServiceOfferingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceOfferingId Id { get; private set; }
    public ServiceOfferingName Name { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public ServiceOfferingStatus Status { get; private set; }
    public OfferingPackageRef? Package { get; private set; }
    public int Version { get; private set; }

    private ServiceOfferingAggregate() { }

    public static ServiceOfferingAggregate Create(
        ServiceOfferingId id,
        ServiceOfferingName name,
        ServiceDefinitionRef serviceDefinition,
        OfferingPackageRef? package = null)
    {
        var aggregate = new ServiceOfferingAggregate();

        var @event = new ServiceOfferingCreatedEvent(id, name, serviceDefinition, package);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ServiceOfferingName name)
    {
        EnsureMutable(nameof(Update));

        var @event = new ServiceOfferingUpdatedEvent(Id, name);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOfferingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceOfferingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ServiceOfferingStatus.Archived)
            throw ServiceOfferingErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceOfferingArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceOfferingCreatedEvent @event)
    {
        Id = @event.ServiceOfferingId;
        Name = @event.Name;
        ServiceDefinition = @event.ServiceDefinition;
        Package = @event.Package;
        Status = ServiceOfferingStatus.Draft;
        Version++;
    }

    private void Apply(ServiceOfferingUpdatedEvent @event)
    {
        Name = @event.Name;
        Version++;
    }

    private void Apply(ServiceOfferingActivatedEvent @event)
    {
        Status = ServiceOfferingStatus.Active;
        Version++;
    }

    private void Apply(ServiceOfferingArchivedEvent @event)
    {
        Status = ServiceOfferingStatus.Archived;
        Version++;
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOfferingErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceOfferingErrors.MissingId();

        if (ServiceDefinition == default)
            throw new ServiceOfferingDomainException("ServiceDefinitionRef is required for a service-offering.");

        if (!Enum.IsDefined(Status))
            throw ServiceOfferingErrors.InvalidStateTransition(Status, "validate");
    }
}
