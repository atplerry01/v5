using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;

namespace Whycespace.Projections.Economic.Revenue.Payout.Reducer;

public static class PayoutProjectionReducer
{
    public static PayoutReadModel Apply(PayoutReadModel state, PayoutExecutedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            Status = "Completed"
        };
}
