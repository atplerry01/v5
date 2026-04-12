namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SpvId Id { get; private set; }
    public SpvDescriptor Descriptor { get; private set; }
    public SpvStatus Status { get; private set; }
    public int Version { get; private set; }

    private SpvAggregate() { }

    public static SpvAggregate Create(SpvId id, SpvDescriptor descriptor)
    {
        var aggregate = new SpvAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SpvCreatedEvent(id, descriptor);
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
            throw SpvErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SpvActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new SpvSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new SpvClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SpvCreatedEvent @event)
    {
        Id = @event.SpvId;
        Descriptor = @event.Descriptor;
        Status = SpvStatus.Created;
        Version++;
    }

    private void Apply(SpvActivatedEvent @event)
    {
        Status = SpvStatus.Active;
        Version++;
    }

    private void Apply(SpvSuspendedEvent @event)
    {
        Status = SpvStatus.Suspended;
        Version++;
    }

    private void Apply(SpvClosedEvent @event)
    {
        Status = SpvStatus.Closed;
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
            throw SpvErrors.MissingId();

        if (Descriptor == default)
            throw SpvErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw SpvErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
