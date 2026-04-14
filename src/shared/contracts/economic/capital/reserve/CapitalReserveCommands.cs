namespace Whycespace.Shared.Contracts.Economic.Capital.Reserve;

public sealed record CreateCapitalReserveCommand(
    Guid ReserveId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset ReservedAt,
    DateTimeOffset ExpiresAt);

public sealed record ReleaseCapitalReserveCommand(
    Guid ReserveId,
    DateTimeOffset ReleasedAt);

public sealed record ExpireCapitalReserveCommand(
    Guid ReserveId,
    DateTimeOffset ExpiredAt);
