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
            Status = "Created",
        };

    // Phase 7 B1 — compensation transitions. OriginalPayoutId is the
    // correlation anchor to the sibling payout whose reversal drove
    // this compensation; preserved from the Requested event through
    // the terminal Compensated row.

    public static DistributionReadModel Apply(DistributionReadModel state, DistributionCompensationRequestedEventSchema e) =>
        state with
        {
            DistributionId = e.AggregateId,
            Status = "CompensationRequested",
            OriginalPayoutId = e.OriginalPayoutId,
            CompensationReason = e.Reason,
            CompensationRequestedAt = e.RequestedAt,
            LastUpdatedAt = e.RequestedAt,
        };

    public static DistributionReadModel Apply(DistributionReadModel state, DistributionCompensatedEventSchema e) =>
        state with
        {
            DistributionId = e.AggregateId,
            Status = "Compensated",
            OriginalPayoutId = e.OriginalPayoutId,
            CompensatingJournalId = e.CompensatingJournalId,
            CompensatedAt = e.CompensatedAt,
            LastUpdatedAt = e.CompensatedAt,
        };
}
