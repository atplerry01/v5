namespace Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Contract;

public sealed record ContractCreatedEventSchema(Guid AggregateId, DateTimeOffset CreatedAt);

public sealed record ContractPartyAddedEventSchema(Guid AggregateId, Guid PartyId, string Role);

public sealed record ContractActivatedEventSchema(Guid AggregateId);

public sealed record ContractSuspendedEventSchema(Guid AggregateId);

public sealed record ContractTerminatedEventSchema(Guid AggregateId);
