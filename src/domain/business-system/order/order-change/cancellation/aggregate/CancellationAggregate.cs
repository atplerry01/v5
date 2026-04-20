using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed class CancellationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CancellationId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public CancellationReason Reason { get; private set; }
    public CancellationStatus Status { get; private set; }
    public int Version { get; private set; }

    private CancellationAggregate() { }

    public static CancellationAggregate Request(
        CancellationId id,
        OrderRef order,
        CancellationReason reason,
        DateTimeOffset requestedAt)
    {
        var aggregate = new CancellationAggregate();

        var @event = new CancellationRequestedEvent(id, order, reason, requestedAt);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm(DateTimeOffset confirmedAt)
    {
        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CancellationErrors.AlreadyTerminal(Id, Status);

        var @event = new CancellationConfirmedEvent(Id, confirmedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CancellationErrors.AlreadyTerminal(Id, Status);

        var @event = new CancellationRejectedEvent(Id, rejectedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CancellationRequestedEvent @event)
    {
        Id = @event.CancellationId;
        Order = @event.Order;
        Reason = @event.Reason;
        Status = CancellationStatus.Requested;
        Version++;
    }

    private void Apply(CancellationConfirmedEvent @event)
    {
        Status = CancellationStatus.Confirmed;
        Version++;
    }

    private void Apply(CancellationRejectedEvent @event)
    {
        Status = CancellationStatus.Rejected;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CancellationErrors.MissingId();

        if (Order == default)
            throw CancellationErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw CancellationErrors.InvalidStateTransition(Status, "validate");
    }
}
