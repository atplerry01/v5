namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Obligation;

public sealed record ObligationCreatedEventSchema(
    Guid AggregateId,
    Guid CounterpartyId,
    string Type,
    decimal Amount,
    string Currency);

public sealed record ObligationFulfilledEventSchema(
    Guid AggregateId,
    Guid SettlementId);

public sealed record ObligationCancelledEventSchema(
    Guid AggregateId,
    string Reason);
