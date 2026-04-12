namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed class DispatchAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public DispatchId Id { get; private set; }
    public ShipmentReference ShipmentReference { get; private set; }
    public DispatchStatus Status { get; private set; }
    public int Version { get; private set; }

    private DispatchAggregate() { }

    public static DispatchAggregate Create(
        DispatchId id,
        ShipmentReference shipmentReference)
    {
        var aggregate = new DispatchAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new DispatchCreatedEvent(id, shipmentReference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Release()
    {
        ValidateBeforeChange();

        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DispatchErrors.InvalidStateTransition(Status, nameof(Release));

        var @event = new DispatchReleasedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DispatchErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new DispatchCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(DispatchCreatedEvent @event)
    {
        Id = @event.DispatchId;
        ShipmentReference = @event.ShipmentReference;
        Status = DispatchStatus.Created;
        Version++;
    }

    private void Apply(DispatchReleasedEvent @event)
    {
        Status = DispatchStatus.Released;
        Version++;
    }

    private void Apply(DispatchCompletedEvent @event)
    {
        Status = DispatchStatus.Completed;
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
            throw DispatchErrors.MissingId();

        if (ShipmentReference == default)
            throw DispatchErrors.ShipmentReferenceRequired();

        if (!Enum.IsDefined(Status))
            throw DispatchErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
