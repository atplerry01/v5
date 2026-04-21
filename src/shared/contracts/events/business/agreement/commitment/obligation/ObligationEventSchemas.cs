namespace Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Obligation;

public sealed record ObligationCreatedEventSchema(Guid AggregateId);

public sealed record ObligationFulfilledEventSchema(Guid AggregateId);

public sealed record ObligationBreachedEventSchema(Guid AggregateId);
