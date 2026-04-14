namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Account;

public sealed record CapitalAccountOpenedEventSchema(
    Guid AggregateId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record CapitalFundedEventSchema(
    Guid AggregateId,
    decimal FundedAmount,
    decimal NewTotalBalance,
    decimal NewAvailableBalance);

public sealed record AccountCapitalAllocatedEventSchema(
    Guid AggregateId,
    decimal AllocatedAmount,
    decimal NewAvailableBalance);

public sealed record AccountCapitalReservedEventSchema(
    Guid AggregateId,
    decimal ReservedAmount,
    decimal NewAvailableBalance,
    decimal NewReservedBalance);

public sealed record AccountReservationReleasedEventSchema(
    Guid AggregateId,
    decimal ReleasedAmount,
    decimal NewAvailableBalance,
    decimal NewReservedBalance);

public sealed record CapitalAccountFrozenEventSchema(
    Guid AggregateId,
    string Reason);

public sealed record CapitalAccountClosedEventSchema(
    Guid AggregateId,
    DateTimeOffset ClosedAt);
