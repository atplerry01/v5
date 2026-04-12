namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed class DeliveryAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public DeliveryId Id { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DeliveryContract Contract { get; private set; }
    public int Version { get; private set; }

    private DeliveryAggregate() { }

    public static DeliveryAggregate Create(DeliveryId id, DeliveryContract contract)
    {
        var aggregate = new DeliveryAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new DeliveryDefinedEvent(id, contract);
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
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new DeliveryActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new DeliverySuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resume()
    {
        ValidateBeforeChange();

        var specification = new CanResumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Resume));

        var @event = new DeliveryResumedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(DeliveryDefinedEvent @event)
    {
        Id = @event.DeliveryId;
        Contract = @event.Contract;
        Status = DeliveryStatus.Draft;
        Version++;
    }

    private void Apply(DeliveryActivatedEvent @event)
    {
        Status = DeliveryStatus.Active;
        Version++;
    }

    private void Apply(DeliverySuspendedEvent @event)
    {
        Status = DeliveryStatus.Suspended;
        Version++;
    }

    private void Apply(DeliveryResumedEvent @event)
    {
        Status = DeliveryStatus.Active;
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
            throw DeliveryErrors.MissingId();

        if (Contract == default)
            throw DeliveryErrors.InvalidContract();

        if (!Enum.IsDefined(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
