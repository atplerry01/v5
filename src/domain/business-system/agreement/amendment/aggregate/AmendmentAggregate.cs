namespace Whycespace.Domain.BusinessSystem.Agreement.Amendment;

public sealed class AmendmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AmendmentId Id { get; private set; }
    public AmendmentTargetId TargetId { get; private set; }
    public AmendmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private AmendmentAggregate() { }

    public static AmendmentAggregate Create(AmendmentId id, AmendmentTargetId targetId)
    {
        var aggregate = new AmendmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AmendmentCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void ApplyAmendment()
    {
        ValidateBeforeChange();

        var specification = new CanApplyAmendmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(ApplyAmendment));

        var @event = new AmendmentAppliedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RevertAmendment()
    {
        ValidateBeforeChange();

        var specification = new CanRevertAmendmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(RevertAmendment));

        var @event = new AmendmentRevertedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AmendmentCreatedEvent @event)
    {
        Id = @event.AmendmentId;
        TargetId = @event.TargetId;
        Status = AmendmentStatus.Draft;
        Version++;
    }

    private void Apply(AmendmentAppliedEvent @event)
    {
        Status = AmendmentStatus.Applied;
        Version++;
    }

    private void Apply(AmendmentRevertedEvent @event)
    {
        Status = AmendmentStatus.Reverted;
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
            throw AmendmentErrors.MissingId();

        if (TargetId == default)
            throw AmendmentErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
