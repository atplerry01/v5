using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed class FareRuleAggregate : AggregateRoot
{
    public FareRuleId Id { get; private set; }
    public TariffRef Tariff { get; private set; }
    public FareRuleCode Code { get; private set; }
    public FareRuleCondition Condition { get; private set; }
    public FareRuleStatus Status { get; private set; }

    public static FareRuleAggregate Create(
        FareRuleId id,
        TariffRef tariff,
        FareRuleCode code,
        FareRuleCondition condition)
    {
        var aggregate = new FareRuleAggregate();
        if (aggregate.Version >= 0)
            throw FareRuleErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new FareRuleCreatedEvent(id, tariff, code, condition));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new FareRuleActivatedEvent(Id));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new FareRuleDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == FareRuleStatus.Archived)
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new FareRuleArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FareRuleCreatedEvent e:
                Id = e.FareRuleId;
                Tariff = e.Tariff;
                Code = e.Code;
                Condition = e.Condition;
                Status = FareRuleStatus.Draft;
                break;
            case FareRuleActivatedEvent:
                Status = FareRuleStatus.Active;
                break;
            case FareRuleDeprecatedEvent:
                Status = FareRuleStatus.Deprecated;
                break;
            case FareRuleArchivedEvent:
                Status = FareRuleStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw FareRuleErrors.MissingId();

        if (Tariff == default)
            throw FareRuleErrors.MissingTariffRef();

        if (!Enum.IsDefined(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, "validate");
    }
}
