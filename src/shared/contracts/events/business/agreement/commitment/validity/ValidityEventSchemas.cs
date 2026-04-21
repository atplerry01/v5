namespace Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Validity;

public sealed record ValidityCreatedEventSchema(Guid AggregateId);

public sealed record ValidityExpiredEventSchema(Guid AggregateId);

public sealed record ValidityInvalidatedEventSchema(Guid AggregateId);
