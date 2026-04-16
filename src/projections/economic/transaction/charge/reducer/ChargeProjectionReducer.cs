using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Charge;

namespace Whycespace.Projections.Economic.Transaction.Charge.Reducer;

public static class ChargeProjectionReducer
{
    public static ChargeReadModel Apply(ChargeReadModel state, ChargeCalculatedEventSchema e) =>
        state with
        {
            ChargeId = e.AggregateId,
            TransactionId = e.TransactionId,
            Type = e.Type,
            BaseAmount = e.BaseAmount,
            ChargeAmount = e.ChargeAmount,
            Currency = e.Currency,
            Status = "Calculated",
            CalculatedAt = e.CalculatedAt
        };

    public static ChargeReadModel Apply(ChargeReadModel state, ChargeAppliedEventSchema e) =>
        state with
        {
            ChargeId = e.AggregateId,
            Status = "Applied",
            AppliedAt = e.AppliedAt
        };
}
