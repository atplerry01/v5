using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Payout;

public sealed class PayoutTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void ExecutePayout_FromDistributionShares_RaisesPayoutExecutedEvent()
    {
        var payoutId = IdGen.Generate("PayoutTests:Execute:payout").ToString();
        var distributionId = IdGen.Generate("PayoutTests:Execute:distribution").ToString();

        var shares = new List<ParticipantShare>
        {
            new("participant-a", 600m, 60m),
            new("participant-b", 400m, 40m),
        };

        var aggregate = PayoutAggregate.ExecutePayout(payoutId, distributionId, shares);

        Assert.Equal(PayoutStatus.Completed, aggregate.Status);
        Assert.Equal(payoutId, aggregate.PayoutId.Value.ToString());
        Assert.Equal(distributionId, aggregate.DistributionId.Value.ToString());

        var evt = Assert.IsType<PayoutExecutedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(payoutId, evt.PayoutId);
        Assert.Equal(distributionId, evt.DistributionId);
    }
}
