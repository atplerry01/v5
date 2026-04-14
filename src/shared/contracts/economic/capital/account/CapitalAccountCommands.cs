namespace Whycespace.Shared.Contracts.Economic.Capital.Account;

public sealed record OpenCapitalAccountCommand(
    Guid AccountId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record FundCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency);

public sealed record AllocateCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency);

public sealed record ReserveCapitalAccountCommand(
    Guid AccountId,
    decimal Amount,
    string Currency);

public sealed record ReleaseCapitalReservationCommand(
    Guid AccountId,
    decimal Amount,
    string Currency);

public sealed record FreezeCapitalAccountCommand(
    Guid AccountId,
    string Reason);

public sealed record CloseCapitalAccountCommand(
    Guid AccountId,
    DateTimeOffset ClosedAt);
