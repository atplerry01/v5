using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed class ServiceOptionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceOptionId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public OptionCode Code { get; private set; }
    public OptionName Name { get; private set; }
    public OptionKind Kind { get; private set; }
    public ServiceOptionStatus Status { get; private set; }
    public int Version { get; private set; }

    private ServiceOptionAggregate() { }

    public static ServiceOptionAggregate Create(
        ServiceOptionId id,
        ServiceDefinitionRef serviceDefinition,
        OptionCode code,
        OptionName name,
        OptionKind kind)
    {
        var aggregate = new ServiceOptionAggregate();

        var @event = new ServiceOptionCreatedEvent(id, serviceDefinition, code, name, kind);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(OptionName name, OptionKind kind)
    {
        EnsureMutable();

        var @event = new ServiceOptionUpdatedEvent(Id, name, kind);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOptionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceOptionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ServiceOptionStatus.Archived)
            throw ServiceOptionErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceOptionArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceOptionCreatedEvent @event)
    {
        Id = @event.ServiceOptionId;
        ServiceDefinition = @event.ServiceDefinition;
        Code = @event.Code;
        Name = @event.Name;
        Kind = @event.Kind;
        Status = ServiceOptionStatus.Draft;
        Version++;
    }

    private void Apply(ServiceOptionUpdatedEvent @event)
    {
        Name = @event.Name;
        Kind = @event.Kind;
        Version++;
    }

    private void Apply(ServiceOptionActivatedEvent @event)
    {
        Status = ServiceOptionStatus.Active;
        Version++;
    }

    private void Apply(ServiceOptionArchivedEvent @event)
    {
        Status = ServiceOptionStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOptionErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceOptionErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceOptionErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceOptionErrors.InvalidStateTransition(Status, "validate");
    }
}
