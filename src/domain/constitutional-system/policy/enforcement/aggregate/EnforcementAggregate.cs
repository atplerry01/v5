using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed class EnforcementAggregate : AggregateRoot
{
    public EnforcementId Id { get; private set; }
    public EnforcementAction Action { get; private set; }
    public EnforcementStatus Status { get; private set; }

    private EnforcementAggregate() { }

    public static EnforcementAggregate Record(EnforcementId id, EnforcementAction action)
    {
        var aggregate = new EnforcementAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.RaiseDomainEvent(new EnforcementRecordedEvent(id, action));
        return aggregate;
    }

    public void ApplyEnforcement()
    {
        ValidateBeforeChange();

        var specification = new CanApplySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, nameof(ApplyEnforcement));

        RaiseDomainEvent(new EnforcementAppliedEvent(Id));
    }

    public void Withdraw()
    {
        ValidateBeforeChange();

        var specification = new CanWithdrawSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, nameof(Withdraw));

        RaiseDomainEvent(new EnforcementWithdrawnEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EnforcementRecordedEvent e:
                Id = e.EnforcementId;
                Action = e.Action;
                Status = EnforcementStatus.Pending;
                break;
            case EnforcementAppliedEvent:
                Status = EnforcementStatus.Applied;
                break;
            case EnforcementWithdrawnEvent:
                Status = EnforcementStatus.Withdrawn;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EnforcementErrors.MissingId();

        if (Action == default)
            throw EnforcementErrors.MissingAction();

        if (!Enum.IsDefined(Status))
            throw EnforcementErrors.InvalidStateTransition(Status, "validate");
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
