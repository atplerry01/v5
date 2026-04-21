using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed class LimitAggregate : AggregateRoot
{
    public LimitId Id { get; private set; }
    public LimitSubjectId SubjectId { get; private set; }
    public LimitStatus Status { get; private set; }
    public int ThresholdValue { get; private set; }

    public static LimitAggregate Create(LimitId id, LimitSubjectId subjectId, int thresholdValue)
    {
        Guard.Against(thresholdValue <= 0, "Threshold value must be greater than zero.");

        var aggregate = new LimitAggregate();
        if (aggregate.Version >= 0)
            throw LimitErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new LimitCreatedEvent(id, subjectId, thresholdValue));
        return aggregate;
    }

    public void Enforce()
    {
        var specification = new CanEnforceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LimitErrors.InvalidStateTransition(Status, nameof(Enforce));

        RaiseDomainEvent(new LimitEnforcedEvent(Id));
    }

    public void Breach(int observedValue)
    {
        Guard.Against(observedValue <= ThresholdValue, "Observed value must exceed threshold to constitute a breach.");

        var specification = new CanBreachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LimitErrors.InvalidStateTransition(Status, nameof(Breach));

        RaiseDomainEvent(new LimitBreachedEvent(Id, observedValue));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LimitCreatedEvent e:
                Id = e.LimitId;
                SubjectId = e.SubjectId;
                ThresholdValue = e.ThresholdValue;
                Status = LimitStatus.Defined;
                break;
            case LimitEnforcedEvent:
                Status = LimitStatus.Enforced;
                break;
            case LimitBreachedEvent:
                Status = LimitStatus.Breached;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw LimitErrors.MissingId();

        if (SubjectId == default)
            throw LimitErrors.MissingSubjectId();

        if (ThresholdValue <= 0)
            throw LimitErrors.InvalidThreshold();

        if (!Enum.IsDefined(Status))
            throw LimitErrors.InvalidStateTransition(Status, "validate");
    }
}
