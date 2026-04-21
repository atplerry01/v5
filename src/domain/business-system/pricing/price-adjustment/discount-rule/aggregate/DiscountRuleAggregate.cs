using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed class DiscountRuleAggregate : AggregateRoot
{
    public DiscountRuleId Id { get; private set; }
    public DiscountRuleCode Code { get; private set; }
    public DiscountRuleName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public DiscountAmount Amount { get; private set; }
    public DiscountRuleStatus Status { get; private set; }

    public static DiscountRuleAggregate Create(
        DiscountRuleId id,
        DiscountRuleCode code,
        DiscountRuleName name,
        AdjustmentBasis basis,
        DiscountAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new DiscountRuleAggregate();
        if (aggregate.Version >= 0)
            throw DiscountRuleErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new DiscountRuleCreatedEvent(id, code, name, basis, amount));
        return aggregate;
    }

    public void Update(DiscountRuleName name, AdjustmentBasis basis, DiscountAmount amount)
    {
        if (Status == DiscountRuleStatus.Archived)
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        RaiseDomainEvent(new DiscountRuleUpdatedEvent(Id, name, basis, amount));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new DiscountRuleActivatedEvent(Id));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new DiscountRuleDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == DiscountRuleStatus.Archived)
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new DiscountRuleArchivedEvent(Id));
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, DiscountAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw DiscountRuleErrors.PercentageOutOfRange(amount.Value);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DiscountRuleCreatedEvent e:
                Id = e.DiscountRuleId;
                Code = e.Code;
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                Status = DiscountRuleStatus.Draft;
                break;
            case DiscountRuleUpdatedEvent e:
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                break;
            case DiscountRuleActivatedEvent:
                Status = DiscountRuleStatus.Active;
                break;
            case DiscountRuleDeprecatedEvent:
                Status = DiscountRuleStatus.Deprecated;
                break;
            case DiscountRuleArchivedEvent:
                Status = DiscountRuleStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw DiscountRuleErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw DiscountRuleErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
