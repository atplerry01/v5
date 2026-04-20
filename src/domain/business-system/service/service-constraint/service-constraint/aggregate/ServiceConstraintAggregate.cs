using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed class ServiceConstraintAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceConstraintId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public ConstraintKind Kind { get; private set; }
    public ConstraintDescriptor Descriptor { get; private set; }
    public ConstraintStatus Status { get; private set; }
    public int Version { get; private set; }

    private ServiceConstraintAggregate() { }

    public static ServiceConstraintAggregate Create(
        ServiceConstraintId id,
        ServiceDefinitionRef serviceDefinition,
        ConstraintKind kind,
        ConstraintDescriptor descriptor)
    {
        var aggregate = new ServiceConstraintAggregate();

        var @event = new ServiceConstraintCreatedEvent(id, serviceDefinition, kind, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ConstraintKind kind, ConstraintDescriptor descriptor)
    {
        EnsureMutable();

        var @event = new ServiceConstraintUpdatedEvent(Id, kind, descriptor);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceConstraintErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceConstraintActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ConstraintStatus.Archived)
            throw ServiceConstraintErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceConstraintArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceConstraintCreatedEvent @event)
    {
        Id = @event.ServiceConstraintId;
        ServiceDefinition = @event.ServiceDefinition;
        Kind = @event.Kind;
        Descriptor = @event.Descriptor;
        Status = ConstraintStatus.Draft;
        Version++;
    }

    private void Apply(ServiceConstraintUpdatedEvent @event)
    {
        Kind = @event.Kind;
        Descriptor = @event.Descriptor;
        Version++;
    }

    private void Apply(ServiceConstraintActivatedEvent @event)
    {
        Status = ConstraintStatus.Active;
        Version++;
    }

    private void Apply(ServiceConstraintArchivedEvent @event)
    {
        Status = ConstraintStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceConstraintErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceConstraintErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceConstraintErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceConstraintErrors.InvalidStateTransition(Status, "validate");
    }
}
