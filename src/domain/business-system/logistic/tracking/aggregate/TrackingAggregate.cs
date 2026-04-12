namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class TrackingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TrackingId Id { get; private set; }
    public ShipmentReference ShipmentReference { get; private set; }
    public TrackingPoint InitialPoint { get; private set; }
    public TrackingStatus Status { get; private set; }
    public int Version { get; private set; }

    private TrackingAggregate() { }

    public static TrackingAggregate Create(
        TrackingId id,
        ShipmentReference shipmentReference,
        TrackingPoint initialPoint)
    {
        var aggregate = new TrackingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TrackingCreatedEvent(id, shipmentReference, initialPoint);
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
            throw TrackingErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TrackingActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Pause()
    {
        ValidateBeforeChange();

        var specification = new CanPauseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TrackingErrors.InvalidStateTransition(Status, nameof(Pause));

        var @event = new TrackingPausedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resume()
    {
        ValidateBeforeChange();

        var specification = new CanResumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TrackingErrors.InvalidStateTransition(Status, nameof(Resume));

        var @event = new TrackingResumedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TrackingErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new TrackingCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TrackingCreatedEvent @event)
    {
        Id = @event.TrackingId;
        ShipmentReference = @event.ShipmentReference;
        InitialPoint = @event.InitialPoint;
        Status = TrackingStatus.Created;
        Version++;
    }

    private void Apply(TrackingActivatedEvent @event)
    {
        Status = TrackingStatus.Active;
        Version++;
    }

    private void Apply(TrackingPausedEvent @event)
    {
        Status = TrackingStatus.Paused;
        Version++;
    }

    private void Apply(TrackingResumedEvent @event)
    {
        Status = TrackingStatus.Active;
        Version++;
    }

    private void Apply(TrackingCompletedEvent @event)
    {
        Status = TrackingStatus.Completed;
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
            throw TrackingErrors.MissingId();

        if (ShipmentReference == default)
            throw TrackingErrors.ShipmentReferenceRequired();

        if (InitialPoint == default)
            throw TrackingErrors.TrackingPointRequired();

        if (!Enum.IsDefined(Status))
            throw TrackingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
