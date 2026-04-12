namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed class SubclusterAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SubclusterId Id { get; private set; }
    public SubclusterDescriptor Descriptor { get; private set; }
    public SubclusterStatus Status { get; private set; }
    public int Version { get; private set; }

    private SubclusterAggregate() { }

    public static SubclusterAggregate Define(SubclusterId id, SubclusterDescriptor descriptor)
    {
        var aggregate = new SubclusterAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SubclusterDefinedEvent(id, descriptor);
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
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SubclusterActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new SubclusterArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SubclusterDefinedEvent @event)
    {
        Id = @event.SubclusterId;
        Descriptor = @event.Descriptor;
        Status = SubclusterStatus.Defined;
        Version++;
    }

    private void Apply(SubclusterActivatedEvent @event)
    {
        Status = SubclusterStatus.Active;
        Version++;
    }

    private void Apply(SubclusterArchivedEvent @event)
    {
        Status = SubclusterStatus.Archived;
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
            throw SubclusterErrors.MissingId();

        if (Descriptor == default)
            throw SubclusterErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
