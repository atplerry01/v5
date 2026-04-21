using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed class AmendmentAggregate : AggregateRoot
{
    public AmendmentId Id { get; private set; }
    public AmendmentTargetId TargetId { get; private set; }
    public AmendmentStatus Status { get; private set; }

    public static AmendmentAggregate Create(AmendmentId id, AmendmentTargetId targetId)
    {
        var aggregate = new AmendmentAggregate();
        if (aggregate.Version >= 0)
            throw AmendmentErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AmendmentCreatedEvent(id, targetId));
        return aggregate;
    }

    public void ApplyAmendment()
    {
        var specification = new CanApplyAmendmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(ApplyAmendment));

        RaiseDomainEvent(new AmendmentAppliedEvent(Id));
    }

    public void RevertAmendment()
    {
        var specification = new CanRevertAmendmentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, nameof(RevertAmendment));

        RaiseDomainEvent(new AmendmentRevertedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AmendmentCreatedEvent e:
                Id = e.AmendmentId;
                TargetId = e.TargetId;
                Status = AmendmentStatus.Draft;
                break;
            case AmendmentAppliedEvent:
                Status = AmendmentStatus.Applied;
                break;
            case AmendmentRevertedEvent:
                Status = AmendmentStatus.Reverted;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AmendmentErrors.MissingId();

        if (TargetId == default)
            throw AmendmentErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw AmendmentErrors.InvalidStateTransition(Status, "validate");
    }
}
