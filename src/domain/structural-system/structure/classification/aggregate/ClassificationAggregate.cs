namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public sealed class ClassificationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ClassificationId Id { get; private set; }
    public ClassificationDescriptor Descriptor { get; private set; }
    public ClassificationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ClassificationAggregate() { }

    public static ClassificationAggregate Define(ClassificationId id, ClassificationDescriptor descriptor)
    {
        var aggregate = new ClassificationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ClassificationDefinedEvent(id, descriptor);
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
            throw ClassificationErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ClassificationActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        ValidateBeforeChange();

        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClassificationErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new ClassificationDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ClassificationDefinedEvent @event)
    {
        Id = @event.ClassificationId;
        Descriptor = @event.Descriptor;
        Status = ClassificationStatus.Defined;
        Version++;
    }

    private void Apply(ClassificationActivatedEvent @event)
    {
        Status = ClassificationStatus.Active;
        Version++;
    }

    private void Apply(ClassificationDeprecatedEvent @event)
    {
        Status = ClassificationStatus.Deprecated;
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
            throw ClassificationErrors.MissingId();

        if (Descriptor == default)
            throw ClassificationErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw ClassificationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
