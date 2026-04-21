using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.FulfillmentInstruction;

namespace Whycespace.Projections.Business.Order.OrderChange.FulfillmentInstruction.Reducer;

public static class FulfillmentInstructionProjectionReducer
{
    public static FulfillmentInstructionReadModel Apply(
        FulfillmentInstructionReadModel state,
        FulfillmentInstructionCreatedEventSchema e) =>
        state with
        {
            FulfillmentInstructionId = e.AggregateId,
            OrderId = e.OrderId,
            LineItemId = e.LineItemId,
            Directive = e.Directive,
            Status = "Draft"
        };

    public static FulfillmentInstructionReadModel Apply(
        FulfillmentInstructionReadModel state,
        FulfillmentInstructionIssuedEventSchema e) =>
        state with
        {
            FulfillmentInstructionId = e.AggregateId,
            Status = "Issued",
            LastUpdatedAt = e.IssuedAt
        };

    public static FulfillmentInstructionReadModel Apply(
        FulfillmentInstructionReadModel state,
        FulfillmentInstructionCompletedEventSchema e) =>
        state with
        {
            FulfillmentInstructionId = e.AggregateId,
            Status = "Completed",
            LastUpdatedAt = e.CompletedAt
        };

    public static FulfillmentInstructionReadModel Apply(
        FulfillmentInstructionReadModel state,
        FulfillmentInstructionRevokedEventSchema e) =>
        state with
        {
            FulfillmentInstructionId = e.AggregateId,
            Status = "Revoked",
            LastUpdatedAt = e.RevokedAt
        };
}
