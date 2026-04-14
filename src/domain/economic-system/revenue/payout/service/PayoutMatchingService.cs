namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutMatchingService
{
    public bool ValidatePayoutMatchesDistribution(PayoutAggregate payout) =>
        payout.DistributionId.Value != Guid.Empty;
}
