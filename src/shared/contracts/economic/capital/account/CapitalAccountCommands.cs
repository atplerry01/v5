using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Account;

public sealed record OpenCapitalAccountCommand(
    Guid AccountId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record FundCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record AllocateCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record ReserveCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record ReleaseCapitalReservationCommand(
    Guid AccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record FreezeCapitalAccountCommand(
    Guid AccountId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record CloseCapitalAccountCommand(
    Guid AccountId,
    DateTimeOffset ClosedAt) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}
