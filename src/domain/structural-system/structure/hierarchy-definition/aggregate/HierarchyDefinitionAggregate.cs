namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public sealed class HierarchyDefinitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public HierarchyDefinitionId Id { get; private set; }
    public HierarchyDefinitionDescriptor Descriptor { get; private set; }
    public HierarchyDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private HierarchyDefinitionAggregate() { }

    public static HierarchyDefinitionAggregate Define(HierarchyDefinitionId id, HierarchyDefinitionDescriptor descriptor)
    {
        var aggregate = new HierarchyDefinitionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new HierarchyDefinitionDefinedEvent(id, descriptor);
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
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, nameof(Validate));

        var @event = new HierarchyDefinitionValidatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Lock()
    {
        ValidateBeforeChange();

        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, nameof(Lock));

        var @event = new HierarchyDefinitionLockedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(HierarchyDefinitionDefinedEvent @event)
    {
        Id = @event.HierarchyDefinitionId;
        Descriptor = @event.Descriptor;
        Status = HierarchyDefinitionStatus.Defined;
        Version++;
    }

    private void Apply(HierarchyDefinitionValidatedEvent @event)
    {
        Status = HierarchyDefinitionStatus.Validated;
        Version++;
    }

    private void Apply(HierarchyDefinitionLockedEvent @event)
    {
        Status = HierarchyDefinitionStatus.Locked;
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
            throw HierarchyDefinitionErrors.MissingId();

        if (Descriptor == default)
            throw HierarchyDefinitionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw HierarchyDefinitionErrors.InvalidStateTransition(Status, "validate");

        if (Descriptor.ParentReference != Guid.Empty && Descriptor.ParentReference == Id.Value)
            throw HierarchyDefinitionErrors.InvalidParentChild();
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
