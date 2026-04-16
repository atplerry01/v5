namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Treasury;

public sealed record TreasuryCreatedEventSchema(
    Guid AggregateId,
    string Currency);

public sealed record TreasuryFundAllocatedEventSchema(
    Guid AggregateId,
    decimal AllocatedAmount,
    decimal NewBalance);

public sealed record TreasuryFundReleasedEventSchema(
    Guid AggregateId,
    decimal ReleasedAmount,
    decimal NewBalance);
