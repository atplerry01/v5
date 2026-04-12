namespace Whycespace.Domain.BusinessSystem.Integration.Delivery;

public sealed class DeliveryAggregate
{
    private readonly List<object> _domainEvents = new();

    public DeliveryId Id { get; private set; }
    public DeliveryDescriptor Descriptor { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private DeliveryAggregate() { }

    public static DeliveryAggregate Schedule(DeliveryId id, DeliveryDescriptor descriptor)
    {
        if (id.Value == Guid.Empty)
            throw DeliveryErrors.MissingId();

        var aggregate = new DeliveryAggregate();
        aggregate.Apply(id, descriptor);
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    public void Dispatch()
    {
        if (!new CanDispatchSpecification().IsSatisfiedBy(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Dispatch));

        Status = DeliveryStatus.Dispatched;
        _domainEvents.Add(new DeliveryDispatchedEvent(Id));
        EnsureInvariants();
    }

    public void Confirm()
    {
        if (!new CanConfirmSpecification().IsSatisfiedBy(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Confirm));

        Status = DeliveryStatus.Confirmed;
        _domainEvents.Add(new DeliveryConfirmedEvent(Id));
        EnsureInvariants();
    }

    public void Fail()
    {
        if (!new CanFailSpecification().IsSatisfiedBy(Status))
            throw DeliveryErrors.InvalidStateTransition(Status, nameof(Fail));

        Status = DeliveryStatus.Failed;
        _domainEvents.Add(new DeliveryFailedEvent(Id));
        EnsureInvariants();
    }

    private void Apply(DeliveryId id, DeliveryDescriptor descriptor)
    {
        Id = id;
        Descriptor = descriptor;
        Status = DeliveryStatus.Scheduled;
        _domainEvents.Add(new DeliveryScheduledEvent(Id, Descriptor));
    }

    private void EnsureInvariants()
    {
        if (Id.Value == Guid.Empty)
            throw DeliveryErrors.MissingId();
    }
}
