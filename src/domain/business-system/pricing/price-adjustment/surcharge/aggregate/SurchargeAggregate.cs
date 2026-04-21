using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed class SurchargeAggregate : AggregateRoot
{
    public SurchargeId Id { get; private set; }
    public SurchargeCode Code { get; private set; }
    public SurchargeName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public SurchargeAmount Amount { get; private set; }
    public SurchargeStatus Status { get; private set; }

    public static SurchargeAggregate Create(
        SurchargeId id,
        SurchargeCode code,
        SurchargeName name,
        AdjustmentBasis basis,
        SurchargeAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new SurchargeAggregate();
        if (aggregate.Version >= 0)
            throw SurchargeErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SurchargeCreatedEvent(id, code, name, basis, amount));
        return aggregate;
    }

    public void Update(SurchargeName name, AdjustmentBasis basis, SurchargeAmount amount)
    {
        if (Status == SurchargeStatus.Archived)
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        RaiseDomainEvent(new SurchargeUpdatedEvent(Id, name, basis, amount));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new SurchargeActivatedEvent(Id));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new SurchargeDeprecatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == SurchargeStatus.Archived)
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new SurchargeArchivedEvent(Id));
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, SurchargeAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw SurchargeErrors.PercentageOutOfRange(amount.Value);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SurchargeCreatedEvent e:
                Id = e.SurchargeId;
                Code = e.Code;
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                Status = SurchargeStatus.Draft;
                break;
            case SurchargeUpdatedEvent e:
                Name = e.Name;
                Basis = e.Basis;
                Amount = e.Amount;
                break;
            case SurchargeActivatedEvent:
                Status = SurchargeStatus.Active;
                break;
            case SurchargeDeprecatedEvent:
                Status = SurchargeStatus.Deprecated;
                break;
            case SurchargeArchivedEvent:
                Status = SurchargeStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SurchargeErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw SurchargeErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
