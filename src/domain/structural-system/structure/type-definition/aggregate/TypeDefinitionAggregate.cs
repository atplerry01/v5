namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed class TypeDefinitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TypeDefinitionId Id { get; private set; }
    public TypeDefinitionDescriptor Descriptor { get; private set; }
    public TypeDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private TypeDefinitionAggregate() { }

    public static TypeDefinitionAggregate Define(TypeDefinitionId id, TypeDefinitionDescriptor descriptor)
    {
        var aggregate = new TypeDefinitionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TypeDefinitionDefinedEvent(id, descriptor);
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
            throw TypeDefinitionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TypeDefinitionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Retire()
    {
        ValidateBeforeChange();

        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TypeDefinitionErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new TypeDefinitionRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TypeDefinitionDefinedEvent @event)
    {
        Id = @event.TypeDefinitionId;
        Descriptor = @event.Descriptor;
        Status = TypeDefinitionStatus.Defined;
        Version++;
    }

    private void Apply(TypeDefinitionActivatedEvent @event)
    {
        Status = TypeDefinitionStatus.Active;
        Version++;
    }

    private void Apply(TypeDefinitionRetiredEvent @event)
    {
        Status = TypeDefinitionStatus.Retired;
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
            throw TypeDefinitionErrors.MissingId();

        if (Descriptor == default)
            throw TypeDefinitionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TypeDefinitionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
