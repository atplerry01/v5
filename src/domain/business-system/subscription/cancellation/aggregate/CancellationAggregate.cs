namespace Whycespace.Domain.BusinessSystem.Subscription.Cancellation;

public sealed class CancellationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CancellationId Id { get; private set; }
    public CancellationRequest Request { get; private set; }
    public CancellationStatus Status { get; private set; }
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private CancellationAggregate() { }

    public static CancellationAggregate RequestCancellation(CancellationId id, CancellationRequest request)
    {
        var aggregate = new CancellationAggregate();
        var @event = new CancellationRequestedEvent(id, request);
        aggregate.Apply(@event);
        aggregate.EnsureInvariants();
        aggregate._uncommittedEvents.Add(@event);
        return aggregate;
    }

    public void Confirm()
    {
        if (!CanConfirmSpecification.IsSatisfiedBy(Status))
            throw CancellationErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new CancellationConfirmedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    private void Apply(CancellationRequestedEvent @event)
    {
        Id = @event.CancellationId;
        Request = @event.Request;
        Status = CancellationStatus.Requested;
    }

    private void Apply(CancellationConfirmedEvent _)
    {
        Status = CancellationStatus.Confirmed;
    }

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CancellationErrors.MissingId();

        if (Request == default)
            throw CancellationErrors.MissingRequest();

        if (!Enum.IsDefined(Status))
            throw CancellationErrors.InvalidStateTransition(Status, "EnsureInvariants");
    }
}
