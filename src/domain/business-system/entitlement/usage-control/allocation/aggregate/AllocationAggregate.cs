namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed class AllocationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AllocationId Id { get; private set; }
    public ResourceId ResourceId { get; private set; }
    public AllocationStatus Status { get; private set; }
    public int RequestedCapacity { get; private set; }
    public int Version { get; private set; }

    private AllocationAggregate() { }

    public static AllocationAggregate Create(AllocationId id, ResourceId resourceId, int requestedCapacity)
    {
        if (requestedCapacity <= 0)
            throw new ArgumentException("Requested capacity must be greater than zero.", nameof(requestedCapacity));

        var aggregate = new AllocationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AllocationCreatedEvent(id, resourceId, requestedCapacity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Allocate()
    {
        ValidateBeforeChange();

        var specification = new CanAllocateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Allocate));

        var @event = new AllocationAllocatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Release()
    {
        ValidateBeforeChange();

        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Release));

        var @event = new AllocationReleasedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AllocationCreatedEvent @event)
    {
        Id = @event.AllocationId;
        ResourceId = @event.ResourceId;
        RequestedCapacity = @event.RequestedCapacity;
        Status = AllocationStatus.Pending;
        Version++;
    }

    private void Apply(AllocationAllocatedEvent @event)
    {
        Status = AllocationStatus.Allocated;
        Version++;
    }

    private void Apply(AllocationReleasedEvent @event)
    {
        Status = AllocationStatus.Released;
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
            throw AllocationErrors.MissingId();

        if (ResourceId == default)
            throw AllocationErrors.MissingResourceId();

        if (!Enum.IsDefined(Status))
            throw AllocationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
