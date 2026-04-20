using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed class ServiceWindowAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ServiceWindowId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public TimeWindow Range { get; private set; }
    public ServiceWindowStatus Status { get; private set; }
    public int Version { get; private set; }

    private ServiceWindowAggregate() { }

    public static ServiceWindowAggregate Create(
        ServiceWindowId id,
        ServiceDefinitionRef serviceDefinition,
        TimeWindow range)
    {
        var aggregate = new ServiceWindowAggregate();

        var @event = new ServiceWindowCreatedEvent(id, serviceDefinition, range);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void UpdateRange(TimeWindow range)
    {
        EnsureMutable();

        var @event = new ServiceWindowUpdatedEvent(Id, range);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceWindowErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ServiceWindowActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ServiceWindowStatus.Archived)
            throw ServiceWindowErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ServiceWindowArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ServiceWindowCreatedEvent @event)
    {
        Id = @event.ServiceWindowId;
        ServiceDefinition = @event.ServiceDefinition;
        Range = @event.Range;
        Status = ServiceWindowStatus.Draft;
        Version++;
    }

    private void Apply(ServiceWindowUpdatedEvent @event)
    {
        Range = @event.Range;
        Version++;
    }

    private void Apply(ServiceWindowActivatedEvent @event)
    {
        Status = ServiceWindowStatus.Active;
        Version++;
    }

    private void Apply(ServiceWindowArchivedEvent @event)
    {
        Status = ServiceWindowStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceWindowErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceWindowErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceWindowErrors.MissingServiceDefinitionRef();

        if (!Range.IsClosed)
            throw ServiceWindowErrors.ClosedWindowRequired();

        if (!Enum.IsDefined(Status))
            throw ServiceWindowErrors.InvalidStateTransition(Status, "validate");
    }
}
