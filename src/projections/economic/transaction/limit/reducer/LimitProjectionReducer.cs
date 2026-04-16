using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Limit;

namespace Whycespace.Projections.Economic.Transaction.Limit.Reducer;

public static class LimitProjectionReducer
{
    public static LimitReadModel Apply(LimitReadModel state, LimitDefinedEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            AccountId = e.AccountId,
            Type = e.Type,
            Threshold = e.Threshold,
            Currency = e.Currency,
            CurrentUtilization = 0m,
            Status = "Active",
            DefinedAt = e.DefinedAt
        };

    public static LimitReadModel Apply(LimitReadModel state, LimitCheckedEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            CurrentUtilization = e.CurrentUtilization,
            LastCheckedAt = e.CheckedAt
        };

    public static LimitReadModel Apply(LimitReadModel state, LimitExceededEventSchema e) =>
        state with
        {
            LimitId = e.AggregateId,
            Status = "Exceeded",
            LastCheckedAt = e.ExceededAt
        };
}
