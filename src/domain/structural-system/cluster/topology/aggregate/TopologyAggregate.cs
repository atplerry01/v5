namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed class TopologyAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TopologyId Id { get; private set; }
    public TopologyDescriptor Descriptor { get; private set; }
    public TopologyStatus Status { get; private set; }
    public int Version { get; private set; }

    private TopologyAggregate() { }

    public static TopologyAggregate Define(TopologyId id, TopologyDescriptor descriptor)
    {
        var aggregate = new TopologyAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TopologyDefinedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Validate()
    {
        ValidateBeforeChange();

        var specification = new CanValidateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyErrors.InvalidStateTransition(Status, nameof(Validate));

        var @event = new TopologyValidatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Lock()
    {
        ValidateBeforeChange();

        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyErrors.InvalidStateTransition(Status, nameof(Lock));

        var @event = new TopologyLockedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TopologyDefinedEvent @event)
    {
        Id = @event.TopologyId;
        Descriptor = @event.Descriptor;
        Status = TopologyStatus.Defined;
        Version++;
    }

    private void Apply(TopologyValidatedEvent @event)
    {
        Status = TopologyStatus.Validated;
        Version++;
    }

    private void Apply(TopologyLockedEvent @event)
    {
        Status = TopologyStatus.Locked;
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
            throw TopologyErrors.MissingId();

        if (Descriptor == default)
            throw TopologyErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TopologyErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
