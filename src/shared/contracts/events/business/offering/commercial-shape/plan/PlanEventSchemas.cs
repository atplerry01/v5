namespace Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Plan;

public sealed record PlanDraftedEventSchema(Guid AggregateId, string PlanName, string PlanTier);

public sealed record PlanActivatedEventSchema(Guid AggregateId);

public sealed record PlanDeprecatedEventSchema(Guid AggregateId);

public sealed record PlanArchivedEventSchema(Guid AggregateId);
