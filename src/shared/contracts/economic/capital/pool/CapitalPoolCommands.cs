namespace Whycespace.Shared.Contracts.Economic.Capital.Pool;

public sealed record CreateCapitalPoolCommand(
    Guid PoolId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record AggregateCapitalToPoolCommand(
    Guid PoolId,
    Guid SourceAccountId,
    decimal Amount);

public sealed record ReduceCapitalFromPoolCommand(
    Guid PoolId,
    Guid SourceAccountId,
    decimal Amount);
