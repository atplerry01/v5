using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed class IncentiveAggregate : AggregateRoot
{
    public IncentiveId IncentiveId { get; private set; } = null!;
    public IncentiveType IncentiveType { get; private set; } = null!;
    public CompensationModel CompensationModel { get; private set; } = null!;
    public MonetaryValue Amount { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public static IncentiveAggregate Create(Guid incentiveId, IncentiveType incentiveType, CompensationModel compensationModel, decimal amount, Currency currency)
    {
        Guard.AgainstDefault(incentiveId);
        Guard.AgainstNull(incentiveType);
        Guard.AgainstNull(compensationModel);
        Guard.AgainstNull(currency);
        Guard.AgainstNegativeOrZero(amount);

        var incentive = new IncentiveAggregate();
        incentive.Apply(new IncentiveGrantedEvent(incentiveId, incentiveType.TypeName, amount, currency.Code));
        incentive.IncentiveType = incentiveType;
        incentive.CompensationModel = compensationModel;
        return incentive;
    }

    public void Adjust(decimal newAmount)
    {
        EnsureInvariant(IsActive, "NOT_ACTIVE", "Cannot adjust a revoked incentive.");
        Guard.AgainstNegativeOrZero(newAmount);

        Apply(new IncentiveAdjustedEvent(IncentiveId.Value, newAmount));
    }

    public void Revoke(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(IsActive, "ALREADY_REVOKED", "Incentive is already revoked.");

        Apply(new IncentiveRevokedEvent(IncentiveId.Value, reason));
    }

    private void Apply(IncentiveGrantedEvent e)
    {
        Id = e.IncentiveId;
        IncentiveId = new IncentiveId(e.IncentiveId);
        Amount = new MonetaryValue(e.Amount);
        Currency = new Currency(e.Currency);
        IsActive = true;
        RaiseDomainEvent(e);
    }

    private void Apply(IncentiveAdjustedEvent e)
    {
        Amount = new MonetaryValue(e.NewAmount);
        RaiseDomainEvent(e);
    }

    private void Apply(IncentiveRevokedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }
}
