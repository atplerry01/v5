using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Cancellation;

namespace Whycespace.Projections.Business.Order.OrderChange.Cancellation.Reducer;

public static class CancellationProjectionReducer
{
    public static CancellationReadModel Apply(CancellationReadModel state, CancellationRequestedEventSchema e) =>
        state with
        {
            CancellationId = e.AggregateId,
            OrderId = e.OrderId,
            Reason = e.Reason,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastUpdatedAt = e.RequestedAt
        };

    public static CancellationReadModel Apply(CancellationReadModel state, CancellationConfirmedEventSchema e) =>
        state with
        {
            CancellationId = e.AggregateId,
            Status = "Confirmed",
            LastUpdatedAt = e.ConfirmedAt
        };

    public static CancellationReadModel Apply(CancellationReadModel state, CancellationRejectedEventSchema e) =>
        state with
        {
            CancellationId = e.AggregateId,
            Status = "Rejected",
            LastUpdatedAt = e.RejectedAt
        };
}
