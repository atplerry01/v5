namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public sealed class AdjustmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AdjustmentId Id { get; private set; }
    public AdjustmentStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public int Version { get; private set; }

    private AdjustmentAggregate() { }

    public static AdjustmentAggregate Create(AdjustmentId id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw AdjustmentErrors.MissingReason();

        var aggregate = new AdjustmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AdjustmentCreatedEvent(id);
        aggregate.Apply(@event, reason);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void ApplyAdjustment()
    {
        ValidateBeforeChange();

        var specification = new CanApplyAdjustmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AdjustmentErrors.InvalidStateTransition(Status, nameof(ApplyAdjustment));

        var @event = new AdjustmentAppliedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void VoidAdjustment()
    {
        ValidateBeforeChange();

        var specification = new CanVoidAdjustmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AdjustmentErrors.InvalidStateTransition(Status, nameof(VoidAdjustment));

        var @event = new AdjustmentVoidedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AdjustmentCreatedEvent @event, string reason)
    {
        Id = @event.AdjustmentId;
        Status = AdjustmentStatus.Draft;
        Reason = reason;
        Version++;
    }

    private void Apply(AdjustmentAppliedEvent @event)
    {
        Status = AdjustmentStatus.Applied;
        Version++;
    }

    private void Apply(AdjustmentVoidedEvent @event)
    {
        Status = AdjustmentStatus.Voided;
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
            throw AdjustmentErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw AdjustmentErrors.InvalidStateTransition(Status, "validate");

        var reasonSpec = new HasReasonSpecification();
        if (!reasonSpec.IsSatisfiedBy(Reason))
            throw AdjustmentErrors.MissingReason();
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
