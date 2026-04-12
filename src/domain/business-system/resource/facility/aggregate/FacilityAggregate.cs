namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public sealed class FacilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public FacilityId Id { get; private set; }
    public FacilityStatus Status { get; private set; }
    public int Version { get; private set; }

    private FacilityAggregate() { }

    public static FacilityAggregate Create(FacilityId id)
    {
        var aggregate = new FacilityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new FacilityCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateFacilitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FacilityErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new FacilityActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseFacilitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FacilityErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new FacilityClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(FacilityCreatedEvent @event)
    {
        Id = @event.FacilityId;
        Status = FacilityStatus.Pending;
        Version++;
    }

    private void Apply(FacilityActivatedEvent @event)
    {
        Status = FacilityStatus.Active;
        Version++;
    }

    private void Apply(FacilityClosedEvent @event)
    {
        Status = FacilityStatus.Closed;
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
            throw FacilityErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw FacilityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
