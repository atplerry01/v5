namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed class ServiceDefinitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceDefinitionId Id { get; private set; }
    public ServiceDefinitionName Name { get; private set; }
    public ServiceCategory Category { get; private set; }
    public ServiceDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private ServiceDefinitionAggregate() { }

    public static ServiceDefinitionAggregate Create(
        ServiceDefinitionId id,
        ServiceDefinitionName name,
        ServiceCategory category)
    {
        var aggregate = new ServiceDefinitionAggregate();

        var @event = new ServiceDefinitionCreatedEvent(id, name, category);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ServiceDefinitionName name, ServiceCategory category)
    {
        EnsureMutable();

        var @event = new ServiceDefinitionUpdatedEvent(Id, name, category);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceDefinitionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ServiceDefinitionStatus.Archived)
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceDefinitionArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceDefinitionCreatedEvent @event)
    {
        Id = @event.ServiceDefinitionId;
        Name = @event.Name;
        Category = @event.Category;
        Status = ServiceDefinitionStatus.Draft;
        Version++;
    }

    private void Apply(ServiceDefinitionUpdatedEvent @event)
    {
        Name = @event.Name;
        Category = @event.Category;
        Version++;
    }

    private void Apply(ServiceDefinitionActivatedEvent @event)
    {
        Status = ServiceDefinitionStatus.Active;
        Version++;
    }

    private void Apply(ServiceDefinitionArchivedEvent @event)
    {
        Status = ServiceDefinitionStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceDefinitionErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceDefinitionErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ServiceDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
