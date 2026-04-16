using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Limit;

public sealed record DefineLimitCommand(
    Guid LimitId,
    Guid AccountId,
    string Type,
    decimal Threshold,
    string Currency,
    DateTimeOffset DefinedAt) : IHasAggregateId
{
    public Guid AggregateId => LimitId;
}

public sealed record CheckLimitCommand(
    Guid LimitId,
    Guid TransactionId,
    decimal TransactionAmount,
    DateTimeOffset CheckedAt) : IHasAggregateId
{
    public Guid AggregateId => LimitId;
}
