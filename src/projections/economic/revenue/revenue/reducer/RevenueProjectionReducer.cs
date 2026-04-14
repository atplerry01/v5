using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Revenue;

namespace Whycespace.Projections.Economic.Revenue.Revenue.Reducer;

public static class RevenueProjectionReducer
{
    public static RevenueReadModel Apply(RevenueReadModel state, RevenueRecordedEventSchema e) =>
        state with
        {
            RevenueId = e.AggregateId,
            SpvId = e.SpvId,
            Amount = e.Amount,
            Currency = e.Currency,
            SourceRef = e.SourceRef,
            Status = "Recorded"
        };
}
