namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public sealed class RebalanceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RebalanceId Id { get; private set; }
    public RebalanceName Name { get; private set; }
    public RebalanceStatus Status { get; private set; }
    public int Version { get; private set; }

    private RebalanceAggregate() { }

    public static RebalanceAggregate Create(
        RebalanceId id,
        RebalanceName name)
    {
        var aggregate = new RebalanceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RebalanceCreatedEvent(id, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Submit()
    {
        ValidateBeforeChange();

        var specification = new CanSubmitSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RebalanceErrors.InvalidStateTransition(Status, nameof(Submit));

        var @event = new RebalanceSubmittedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Approve()
    {
        ValidateBeforeChange();

        var specification = new CanApproveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RebalanceErrors.InvalidStateTransition(Status, nameof(Approve));

        var @event = new RebalanceApprovedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject()
    {
        ValidateBeforeChange();

        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RebalanceErrors.InvalidStateTransition(Status, nameof(Reject));

        var @event = new RebalanceRejectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RebalanceErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new RebalanceCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RebalanceCreatedEvent @event)
    {
        Id = @event.RebalanceId;
        Name = @event.RebalanceName;
        Status = RebalanceStatus.Draft;
        Version++;
    }

    private void Apply(RebalanceSubmittedEvent @event)
    {
        Status = RebalanceStatus.Pending;
        Version++;
    }

    private void Apply(RebalanceApprovedEvent @event)
    {
        Status = RebalanceStatus.Approved;
        Version++;
    }

    private void Apply(RebalanceRejectedEvent @event)
    {
        Status = RebalanceStatus.Rejected;
        Version++;
    }

    private void Apply(RebalanceCancelledEvent @event)
    {
        Status = RebalanceStatus.Cancelled;
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
            throw RebalanceErrors.MissingId();

        if (Name == default)
            throw RebalanceErrors.NameRequired();

        if (!Enum.IsDefined(Status))
            throw RebalanceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
