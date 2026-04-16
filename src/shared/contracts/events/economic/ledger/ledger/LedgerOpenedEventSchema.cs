namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;

public sealed record LedgerOpenedEventSchema(
    Guid AggregateId,
    string Currency);
