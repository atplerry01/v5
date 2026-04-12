namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class LifecycleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LifecycleId Id { get; private set; }
    public LifecycleDescriptor Descriptor { get; private set; }
    public LifecycleStatus Status { get; private set; }
    public int Version { get; private set; }

    private LifecycleAggregate() { }

    public static LifecycleAggregate Define(LifecycleId id, LifecycleDescriptor descriptor)
    {
        var aggregate = new LifecycleAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new LifecycleDefinedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Transition()
    {
        ValidateBeforeChange();

        var specification = new CanTransitionSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Transition));

        var @event = new LifecycleTransitionedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new LifecycleCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LifecycleDefinedEvent @event)
    {
        Id = @event.LifecycleId;
        Descriptor = @event.Descriptor;
        Status = LifecycleStatus.Defined;
        Version++;
    }

    private void Apply(LifecycleTransitionedEvent @event)
    {
        Status = LifecycleStatus.Transitioned;
        Version++;
    }

    private void Apply(LifecycleCompletedEvent @event)
    {
        Status = LifecycleStatus.Completed;
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
            throw LifecycleErrors.MissingId();

        if (Descriptor == default)
            throw LifecycleErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
