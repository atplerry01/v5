using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;

namespace Whycespace.Projections.Economic.Revenue.Payout.Reducer;

public static class PayoutProjectionReducer
{
    public static PayoutReadModel Apply(PayoutReadModel state, PayoutRequestedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            IdempotencyKey = e.IdempotencyKey,
            Status = "Requested",
            LastUpdatedAt = e.RequestedAt,
        };

    public static PayoutReadModel Apply(PayoutReadModel state, PayoutExecutedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            IdempotencyKey = e.IdempotencyKey,
            Status = "Executed",
            LastUpdatedAt = e.ExecutedAt,
        };

    public static PayoutReadModel Apply(PayoutReadModel state, PayoutFailedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            Status = "Failed",
            LastUpdatedAt = e.FailedAt,
        };

    // Phase 7 B2 — compensation transitions. Preserves every prior
    // field (IdempotencyKey, DistributionId) and layers compensation
    // metadata on top, so full replay produces the same terminal row
    // regardless of whether the stream reaches Compensated via Executed
    // or Failed.

    public static PayoutReadModel Apply(PayoutReadModel state, PayoutCompensationRequestedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            IdempotencyKey = e.IdempotencyKey,
            Status = "CompensationRequested",
            CompensationReason = e.Reason,
            CompensationRequestedAt = e.RequestedAt,
            LastUpdatedAt = e.RequestedAt,
        };

    public static PayoutReadModel Apply(PayoutReadModel state, PayoutCompensatedEventSchema e) =>
        state with
        {
            PayoutId = e.AggregateId,
            DistributionId = e.DistributionId,
            IdempotencyKey = e.IdempotencyKey,
            Status = "Compensated",
            CompensatingJournalId = e.CompensatingJournalId,
            CompensatedAt = e.CompensatedAt,
            LastUpdatedAt = e.CompensatedAt,
        };
}
