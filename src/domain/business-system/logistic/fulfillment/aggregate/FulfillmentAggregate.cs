namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class FulfillmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public FulfillmentId Id { get; private set; }
    public ShipmentReference ShipmentReference { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private FulfillmentAggregate() { }

    public static FulfillmentAggregate Create(FulfillmentId id, ShipmentReference shipmentReference)
    {
        var specification = new FulfillmentSpecification();
        if (!specification.IsSatisfiedBy(id, shipmentReference))
        {
            if (id == default)
                throw FulfillmentErrors.MissingId();
            if (shipmentReference == default)
                throw FulfillmentErrors.MissingShipmentReference();
        }

        var aggregate = new FulfillmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new FulfillmentCreatedEvent(id, shipmentReference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Complete()
    {
        if (Status == FulfillmentStatus.Completed)
            throw FulfillmentErrors.AlreadyCompleted();

        ValidateBeforeChange();

        var completedEvent = new FulfillmentCompletedEvent(Id);
        Apply(completedEvent);
        AddEvent(completedEvent);

        EnsureInvariants();
    }

    private void Apply(FulfillmentCreatedEvent @event)
    {
        Id = @event.FulfillmentId;
        ShipmentReference = @event.ShipmentReference;
        Status = FulfillmentStatus.Created;
        Version++;
    }

    private void Apply(FulfillmentCompletedEvent @event)
    {
        Status = FulfillmentStatus.Completed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw FulfillmentErrors.MissingId();

        if (ShipmentReference == default)
            throw FulfillmentErrors.MissingShipmentReference();

        if (!Enum.IsDefined(Status))
            throw new InvalidOperationException("FulfillmentStatus is not a defined enum value.");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
