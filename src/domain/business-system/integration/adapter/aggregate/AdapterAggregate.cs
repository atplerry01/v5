namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public sealed class AdapterAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AdapterId Id { get; private set; }
    public AdapterTypeId TypeId { get; private set; }
    public AdapterStatus Status { get; private set; }
    public int Version { get; private set; }

    private AdapterAggregate() { }

    public static AdapterAggregate Create(AdapterId id, AdapterTypeId typeId)
    {
        var aggregate = new AdapterAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AdapterCreatedEvent(id, typeId);
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
            throw AdapterErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new AdapterActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AdapterErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new AdapterDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AdapterCreatedEvent @event)
    {
        Id = @event.AdapterId;
        TypeId = @event.TypeId;
        Status = AdapterStatus.Defined;
        Version++;
    }

    private void Apply(AdapterActivatedEvent @event)
    {
        Status = AdapterStatus.Active;
        Version++;
    }

    private void Apply(AdapterDisabledEvent @event)
    {
        Status = AdapterStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AdapterErrors.MissingId();

        if (TypeId == default)
            throw AdapterErrors.MissingTypeId();

        if (!Enum.IsDefined(Status))
            throw AdapterErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
