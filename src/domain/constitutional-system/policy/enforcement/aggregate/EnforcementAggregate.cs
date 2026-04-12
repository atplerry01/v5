namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed class EnforcementAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EnforcementId Id { get; private set; }
    public EnforcementAction Action { get; private set; }
    public EnforcementStatus Status { get; private set; }
    public int Version { get; private set; }

    private EnforcementAggregate() { }

    public static EnforcementAggregate Record(EnforcementId id, EnforcementAction action)
    {
        var aggregate = new EnforcementAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EnforcementRecordedEvent(id, action);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void ApplyEnforcement()
    {
        ValidateBeforeChange();

        var specification = new CanApplySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, nameof(ApplyEnforcement));

        var @event = new EnforcementAppliedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Withdraw()
    {
        ValidateBeforeChange();

        var specification = new CanWithdrawSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, nameof(Withdraw));

        var @event = new EnforcementWithdrawnEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EnforcementRecordedEvent @event)
    {
        Id = @event.EnforcementId;
        Action = @event.Action;
        Status = EnforcementStatus.Pending;
        Version++;
    }

    private void Apply(EnforcementAppliedEvent @event)
    {
        Status = EnforcementStatus.Applied;
        Version++;
    }

    private void Apply(EnforcementWithdrawnEvent @event)
    {
        Status = EnforcementStatus.Withdrawn;
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
            throw EnforcementErrors.MissingId();

        if (Action == default)
            throw EnforcementErrors.MissingAction();

        if (!Enum.IsDefined(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
