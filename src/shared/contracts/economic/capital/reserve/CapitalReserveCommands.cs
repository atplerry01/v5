using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Reserve;

public sealed record CreateCapitalReserveCommand(
    Guid ReserveId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset ReservedAt,
    DateTimeOffset ExpiresAt) : IHasAggregateId
{
    public Guid AggregateId => ReserveId;
}

public sealed record ReleaseCapitalReserveCommand(
    Guid ReserveId,
    DateTimeOffset ReleasedAt) : IHasAggregateId
{
    public Guid AggregateId => ReserveId;
}

public sealed record ExpireCapitalReserveCommand(
    Guid ReserveId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => ReserveId;
}
