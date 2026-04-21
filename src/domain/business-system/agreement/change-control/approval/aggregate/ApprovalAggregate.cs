using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public sealed class ApprovalAggregate : AggregateRoot
{
    public ApprovalId Id { get; private set; }
    public ApprovalStatus Status { get; private set; }

    public static ApprovalAggregate Create(ApprovalId id)
    {
        var aggregate = new ApprovalAggregate();
        if (aggregate.Version >= 0)
            throw ApprovalErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ApprovalCreatedEvent(id));
        return aggregate;
    }

    public void Approve()
    {
        var specification = new CanApproveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, nameof(Approve));

        RaiseDomainEvent(new ApprovalApprovedEvent(Id));
    }

    public void Reject()
    {
        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, nameof(Reject));

        RaiseDomainEvent(new ApprovalRejectedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ApprovalCreatedEvent e:
                Id = e.ApprovalId;
                Status = ApprovalStatus.Pending;
                break;
            case ApprovalApprovedEvent:
                Status = ApprovalStatus.Approved;
                break;
            case ApprovalRejectedEvent:
                Status = ApprovalStatus.Rejected;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ApprovalErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ApprovalErrors.InvalidStateTransition(Status, "validate");
    }
}
