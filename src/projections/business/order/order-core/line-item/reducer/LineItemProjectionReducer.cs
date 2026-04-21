using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.LineItem;

namespace Whycespace.Projections.Business.Order.OrderCore.LineItem.Reducer;

public static class LineItemProjectionReducer
{
    public static LineItemReadModel Apply(LineItemReadModel state, LineItemCreatedEventSchema e) =>
        state with
        {
            LineItemId = e.AggregateId,
            OrderId = e.OrderId,
            SubjectKind = e.SubjectKind,
            SubjectId = e.SubjectId,
            QuantityValue = e.QuantityValue,
            QuantityUnit = e.QuantityUnit,
            Status = "Requested"
        };

    public static LineItemReadModel Apply(LineItemReadModel state, LineItemUpdatedEventSchema e) =>
        state with
        {
            LineItemId = e.AggregateId,
            QuantityValue = e.QuantityValue,
            QuantityUnit = e.QuantityUnit
        };

    public static LineItemReadModel Apply(LineItemReadModel state, LineItemCancelledEventSchema e) =>
        state with
        {
            LineItemId = e.AggregateId,
            Status = "Cancelled"
        };
}
