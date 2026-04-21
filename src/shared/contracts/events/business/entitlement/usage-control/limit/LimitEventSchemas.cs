namespace Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Limit;

public sealed record LimitCreatedEventSchema(Guid AggregateId, Guid SubjectId, int ThresholdValue);

public sealed record LimitEnforcedEventSchema(Guid AggregateId);

public sealed record LimitBreachedEventSchema(Guid AggregateId, int ObservedValue);
