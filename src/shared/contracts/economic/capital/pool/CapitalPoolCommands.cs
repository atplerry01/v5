using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Pool;

public sealed record CreateCapitalPoolCommand(
    Guid PoolId,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => PoolId;
}

public sealed record AggregateCapitalToPoolCommand(
    Guid PoolId,
    Guid SourceAccountId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => PoolId;
}

public sealed record ReduceCapitalFromPoolCommand(
    Guid PoolId,
    Guid SourceAccountId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => PoolId;
}
