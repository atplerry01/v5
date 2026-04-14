using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;

namespace Whycespace.Projections.Economic.Revenue.Distribution.Reducer;

public static class DistributionProjectionReducer
{
    public static DistributionReadModel Apply(DistributionReadModel state, DistributionCreatedEventSchema e) =>
        state with
        {
            DistributionId = e.AggregateId,
            SpvId = e.SpvId,
            TotalAmount = e.TotalAmount,
            Status = "Created"
        };
}
