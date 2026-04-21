namespace Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Counterparty;

public sealed record CounterpartyCreatedEventSchema(Guid AggregateId);

public sealed record CounterpartySuspendedEventSchema(Guid AggregateId);

public sealed record CounterpartyTerminatedEventSchema(Guid AggregateId);
