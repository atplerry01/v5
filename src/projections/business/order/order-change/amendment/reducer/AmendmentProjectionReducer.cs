using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Amendment;

namespace Whycespace.Projections.Business.Order.OrderChange.Amendment.Reducer;

public static class AmendmentProjectionReducer
{
    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentRequestedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            OrderId = e.OrderId,
            Reason = e.Reason,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastUpdatedAt = e.RequestedAt
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentAcceptedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Accepted",
            LastUpdatedAt = e.AcceptedAt
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentAppliedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Applied",
            LastUpdatedAt = e.AppliedAt
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentRejectedEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Rejected",
            LastUpdatedAt = e.RejectedAt
        };

    public static AmendmentReadModel Apply(AmendmentReadModel state, AmendmentCancelledEventSchema e) =>
        state with
        {
            AmendmentId = e.AggregateId,
            Status = "Cancelled",
            LastUpdatedAt = e.CancelledAt
        };
}
