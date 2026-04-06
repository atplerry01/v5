namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutEligibleSpecification
{
    public bool IsSatisfiedBy(PayoutAggregate payout)
    {
        return payout.Status == PayoutStatus.Approved
            && payout.Amount.Value > 0;
    }
}
