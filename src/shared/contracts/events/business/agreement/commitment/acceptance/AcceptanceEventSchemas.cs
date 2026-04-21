namespace Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Acceptance;

public sealed record AcceptanceCreatedEventSchema(Guid AggregateId);

public sealed record AcceptanceAcceptedEventSchema(Guid AggregateId);

public sealed record AcceptanceRejectedEventSchema(Guid AggregateId);
