using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class AmendmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AmendmentId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public AmendmentReason Reason { get; private set; }
    public AmendmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private AmendmentAggregate() { }

    public static AmendmentAggregate Request(
        AmendmentId id,
        OrderRef order,
        AmendmentReason reason,
        DateTimeOffset requestedAt)
    {
        var aggregate = new AmendmentAggregate();

        var @event = new AmendmentRequestedEvent(id, order, reason, requestedAt);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Accept(DateTimeOffset acceptedAt)
    {
        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        var @event = new AmendmentAcceptedEvent(Id, acceptedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkApplied(DateTimeOffset appliedAt)
    {
        var specification = new CanApplySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(MarkApplied));

        var @event = new AmendmentAppliedEvent(Id, appliedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject(DateTimeOffset rejectedAt)
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        var @event = new AmendmentRejectedEvent(Id, rejectedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.AlreadyTerminal(Id, Status);

        var @event = new AmendmentCancelledEvent(Id, cancelledAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AmendmentRequestedEvent @event)
    {
        Id = @event.AmendmentId;
        Order = @event.Order;
        Reason = @event.Reason;
        Status = AmendmentStatus.Requested;
        Version++;
    }

    private void Apply(AmendmentAcceptedEvent @event)
    {
        Status = AmendmentStatus.Accepted;
        Version++;
    }

    private void Apply(AmendmentAppliedEvent @event)
    {
        Status = AmendmentStatus.Applied;
        Version++;
    }

    private void Apply(AmendmentRejectedEvent @event)
    {
        Status = AmendmentStatus.Rejected;
        Version++;
    }

    private void Apply(AmendmentCancelledEvent @event)
    {
        Status = AmendmentStatus.Cancelled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AmendmentErrors.MissingId();

        if (Order == default)
            throw AmendmentErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, "validate");
    }
}
