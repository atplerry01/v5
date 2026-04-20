namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public sealed class ApprovalAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ApprovalId Id { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public int Version { get; private set; }

    private ApprovalAggregate() { }

    public static ApprovalAggregate Create(ApprovalId id)
    {
        var aggregate = new ApprovalAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ApprovalCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Approve()
    {
        ValidateBeforeChange();

        var specification = new CanApproveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, nameof(Approve));

        var @event = new ApprovalApprovedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject()
    {
        ValidateBeforeChange();

        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, nameof(Reject));

        var @event = new ApprovalRejectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ApprovalCreatedEvent @event)
    {
        Id = @event.ApprovalId;
        Status = ApprovalStatus.Pending;
        Version++;
    }

    private void Apply(ApprovalApprovedEvent @event)
    {
        Status = ApprovalStatus.Approved;
        Version++;
    }

    private void Apply(ApprovalRejectedEvent @event)
    {
        Status = ApprovalStatus.Rejected;
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
            throw ApprovalErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
