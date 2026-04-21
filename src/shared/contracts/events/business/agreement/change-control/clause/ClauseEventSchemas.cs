namespace Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Clause;

public sealed record ClauseCreatedEventSchema(Guid AggregateId, string ClauseType);

public sealed record ClauseActivatedEventSchema(Guid AggregateId);

public sealed record ClauseSupersededEventSchema(Guid AggregateId);
