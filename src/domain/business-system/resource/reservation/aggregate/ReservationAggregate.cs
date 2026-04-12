namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class ReservationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReservationId Id { get; private set; }
    public ReservationStatus Status { get; private set; }
    public ResourceReference ResourceReference { get; private set; }
    public ReservedCapacity ReservedCapacity { get; private set; }
    public int Version { get; private set; }

    private ReservationAggregate() { }

    public static ReservationAggregate Create(
        ReservationId id,
        ResourceReference resourceReference,
        ReservedCapacity reservedCapacity)
    {
        var aggregate = new ReservationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ReservationCreatedEvent(id, resourceReference, reservedCapacity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new ReservationConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Release()
    {
        ValidateBeforeChange();

        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.InvalidStateTransition(Status, nameof(Release));

        var @event = new ReservationReleasedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new ReservationCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReservationCreatedEvent @event)
    {
        Id = @event.ReservationId;
        ResourceReference = @event.ResourceReference;
        ReservedCapacity = @event.ReservedCapacity;
        Status = ReservationStatus.Pending;
        Version++;
    }

    private void Apply(ReservationConfirmedEvent @event)
    {
        Status = ReservationStatus.Confirmed;
        Version++;
    }

    private void Apply(ReservationReleasedEvent @event)
    {
        Status = ReservationStatus.Released;
        Version++;
    }

    private void Apply(ReservationCancelledEvent @event)
    {
        Status = ReservationStatus.Cancelled;
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
            throw ReservationErrors.MissingId();

        if (ResourceReference == default)
            throw ReservationErrors.ResourceReferenceRequired();

        if (ReservedCapacity == default)
            throw ReservationErrors.CapacityMustBePositive();

        if (!Enum.IsDefined(Status))
            throw ReservationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
