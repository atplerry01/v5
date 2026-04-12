namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class MaintenanceResourceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MaintenanceResourceId Id { get; private set; }
    public MaintenanceResourceStatus Status { get; private set; }
    public ResourceLink ResourceLink { get; private set; }
    public MaintenanceRequirement Requirement { get; private set; }
    public int Version { get; private set; }

    private MaintenanceResourceAggregate() { }

    public static MaintenanceResourceAggregate Create(
        MaintenanceResourceId id,
        ResourceLink resourceLink,
        MaintenanceRequirement requirement)
    {
        var aggregate = new MaintenanceResourceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MaintenanceResourceCreatedEvent(id, resourceLink, requirement);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MaintenanceResourceErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new MaintenanceResourceActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MaintenanceResourceErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new MaintenanceResourceSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resume()
    {
        ValidateBeforeChange();

        var specification = new CanResumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MaintenanceResourceErrors.InvalidStateTransition(Status, nameof(Resume));

        var @event = new MaintenanceResourceActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MaintenanceResourceErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new MaintenanceResourceCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(MaintenanceResourceCreatedEvent @event)
    {
        Id = @event.MaintenanceResourceId;
        ResourceLink = @event.ResourceLink;
        Requirement = @event.Requirement;
        Status = MaintenanceResourceStatus.Defined;
        Version++;
    }

    private void Apply(MaintenanceResourceActivatedEvent @event)
    {
        Status = MaintenanceResourceStatus.Active;
        Version++;
    }

    private void Apply(MaintenanceResourceSuspendedEvent @event)
    {
        Status = MaintenanceResourceStatus.Suspended;
        Version++;
    }

    private void Apply(MaintenanceResourceCompletedEvent @event)
    {
        Status = MaintenanceResourceStatus.Completed;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MaintenanceResourceErrors.MissingId();

        if (ResourceLink == default)
            throw MaintenanceResourceErrors.ResourceLinkRequired();

        if (Requirement == default)
            throw MaintenanceResourceErrors.RequirementRequired();

        if (!Enum.IsDefined(Status))
            throw MaintenanceResourceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
