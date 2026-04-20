using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed class ServiceLevelAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceLevelId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public LevelCode Code { get; private set; }
    public LevelName Name { get; private set; }
    public ServiceLevelTarget Target { get; private set; }
    public ServiceLevelStatus Status { get; private set; }
    public int Version { get; private set; }

    private ServiceLevelAggregate() { }

    public static ServiceLevelAggregate Create(
        ServiceLevelId id,
        ServiceDefinitionRef serviceDefinition,
        LevelCode code,
        LevelName name,
        ServiceLevelTarget target)
    {
        var aggregate = new ServiceLevelAggregate();

        var @event = new ServiceLevelCreatedEvent(id, serviceDefinition, code, name, target);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(LevelName name, ServiceLevelTarget target)
    {
        EnsureMutable();

        var @event = new ServiceLevelUpdatedEvent(Id, name, target);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceLevelErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceLevelActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ServiceLevelStatus.Archived)
            throw ServiceLevelErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceLevelArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceLevelCreatedEvent @event)
    {
        Id = @event.ServiceLevelId;
        ServiceDefinition = @event.ServiceDefinition;
        Code = @event.Code;
        Name = @event.Name;
        Target = @event.Target;
        Status = ServiceLevelStatus.Draft;
        Version++;
    }

    private void Apply(ServiceLevelUpdatedEvent @event)
    {
        Name = @event.Name;
        Target = @event.Target;
        Version++;
    }

    private void Apply(ServiceLevelActivatedEvent @event)
    {
        Status = ServiceLevelStatus.Active;
        Version++;
    }

    private void Apply(ServiceLevelArchivedEvent @event)
    {
        Status = ServiceLevelStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceLevelErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceLevelErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceLevelErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceLevelErrors.InvalidStateTransition(Status, "validate");
    }
}
