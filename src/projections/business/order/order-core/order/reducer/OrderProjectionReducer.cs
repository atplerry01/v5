using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Order;

namespace Whycespace.Projections.Business.Order.OrderCore.Order.Reducer;

public static class OrderProjectionReducer
{
    public static OrderReadModel Apply(OrderReadModel state, OrderCreatedEventSchema e) =>
        state with
        {
            OrderId = e.AggregateId,
            SourceReferenceId = e.SourceReferenceId,
            Description = e.Description,
            Status = "Created"
        };

    public static OrderReadModel Apply(OrderReadModel state, OrderConfirmedEventSchema e) =>
        state with
        {
            OrderId = e.AggregateId,
            Status = "Confirmed"
        };

    public static OrderReadModel Apply(OrderReadModel state, OrderCompletedEventSchema e) =>
        state with
        {
            OrderId = e.AggregateId,
            Status = "Completed"
        };

    public static OrderReadModel Apply(OrderReadModel state, OrderCancelledEventSchema e) =>
        state with
        {
            OrderId = e.AggregateId,
            Status = "Cancelled",
            CancelledAt = e.CancelledAt
        };
}
