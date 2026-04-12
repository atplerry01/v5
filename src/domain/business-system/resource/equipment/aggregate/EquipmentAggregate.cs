namespace Whycespace.Domain.BusinessSystem.Resource.Equipment;

public sealed class EquipmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EquipmentId Id { get; private set; }
    public EquipmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private EquipmentAggregate() { }

    public static EquipmentAggregate Create(EquipmentId id)
    {
        var aggregate = new EquipmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EquipmentCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateEquipmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EquipmentErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new EquipmentActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Retire()
    {
        ValidateBeforeChange();

        var specification = new CanRetireEquipmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EquipmentErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new EquipmentRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EquipmentCreatedEvent @event)
    {
        Id = @event.EquipmentId;
        Status = EquipmentStatus.Pending;
        Version++;
    }

    private void Apply(EquipmentActivatedEvent @event)
    {
        Status = EquipmentStatus.Active;
        Version++;
    }

    private void Apply(EquipmentRetiredEvent @event)
    {
        Status = EquipmentStatus.Retired;
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
            throw EquipmentErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw EquipmentErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
