namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public sealed class MappingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MappingId Id { get; private set; }
    public MappingDefinitionId DefinitionId { get; private set; }
    public MappingStatus Status { get; private set; }
    public int Version { get; private set; }

    private MappingAggregate() { }

    public static MappingAggregate Create(MappingId id, MappingDefinitionId definitionId)
    {
        var aggregate = new MappingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MappingCreatedEvent(id, definitionId);
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
            throw MappingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new MappingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MappingErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new MappingDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(MappingCreatedEvent @event)
    {
        Id = @event.MappingId;
        DefinitionId = @event.DefinitionId;
        Status = MappingStatus.Defined;
        Version++;
    }

    private void Apply(MappingActivatedEvent @event)
    {
        Status = MappingStatus.Active;
        Version++;
    }

    private void Apply(MappingDisabledEvent @event)
    {
        Status = MappingStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MappingErrors.MissingId();

        if (DefinitionId == default)
            throw MappingErrors.MissingDefinitionId();

        if (!Enum.IsDefined(Status))
            throw MappingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
