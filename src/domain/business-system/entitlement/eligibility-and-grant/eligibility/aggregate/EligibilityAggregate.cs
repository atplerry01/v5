using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class EligibilityAggregate : AggregateRoot
{
    public EligibilityId Id { get; private set; }
    public EligibilitySubjectRef Subject { get; private set; }
    public EligibilityTargetRef Target { get; private set; }
    public EligibilityScope Scope { get; private set; }
    public EligibilityStatus Status { get; private set; }
    public IneligibilityReason? Reason { get; private set; }

    public static EligibilityAggregate Create(
        EligibilityId id,
        EligibilitySubjectRef subject,
        EligibilityTargetRef target,
        EligibilityScope scope)
    {
        var aggregate = new EligibilityAggregate();
        if (aggregate.Version >= 0)
            throw EligibilityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EligibilityCreatedEvent(id, subject, target, scope));
        return aggregate;
    }

    public void MarkEligible(DateTimeOffset evaluatedAt)
    {
        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.AlreadyEvaluated(Id, Status);

        RaiseDomainEvent(new EligibilityEvaluatedEligibleEvent(Id, evaluatedAt));
    }

    public void MarkIneligible(IneligibilityReason reason, DateTimeOffset evaluatedAt)
    {
        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.AlreadyEvaluated(Id, Status);

        RaiseDomainEvent(new EligibilityEvaluatedIneligibleEvent(Id, reason, evaluatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EligibilityCreatedEvent e:
                Id = e.EligibilityId;
                Subject = e.Subject;
                Target = e.Target;
                Scope = e.Scope;
                Status = EligibilityStatus.Pending;
                break;
            case EligibilityEvaluatedEligibleEvent:
                Status = EligibilityStatus.Eligible;
                break;
            case EligibilityEvaluatedIneligibleEvent e:
                Status = EligibilityStatus.Ineligible;
                Reason = e.Reason;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EligibilityErrors.MissingId();

        if (Subject == default)
            throw EligibilityErrors.MissingSubject();

        if (Target == default)
            throw EligibilityErrors.MissingTarget();

        if (!Enum.IsDefined(Status))
            throw EligibilityErrors.InvalidStateTransition(Status, "validate");
    }
}
