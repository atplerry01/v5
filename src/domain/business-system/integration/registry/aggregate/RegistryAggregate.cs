namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public sealed class RegistryAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RegistryId Id { get; private set; }
    public RegistryEntryId EntryId { get; private set; }
    public RegistryStatus Status { get; private set; }
    public int Version { get; private set; }

    private RegistryAggregate() { }

    public static RegistryAggregate Create(RegistryId id, RegistryEntryId entryId)
    {
        var aggregate = new RegistryAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RegistryCreatedEvent(id, entryId);
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
            throw RegistryErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new RegistryActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RegistryErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new RegistryDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RegistryCreatedEvent @event)
    {
        Id = @event.RegistryId;
        EntryId = @event.EntryId;
        Status = RegistryStatus.Defined;
        Version++;
    }

    private void Apply(RegistryActivatedEvent @event)
    {
        Status = RegistryStatus.Active;
        Version++;
    }

    private void Apply(RegistryDeactivatedEvent @event)
    {
        Status = RegistryStatus.Deactivated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RegistryErrors.MissingId();

        if (EntryId == default)
            throw RegistryErrors.MissingEntryId();

        if (!Enum.IsDefined(Status))
            throw RegistryErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
