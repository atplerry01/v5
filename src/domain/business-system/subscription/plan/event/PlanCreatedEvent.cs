namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public sealed record PlanDraftedEvent(PlanId PlanId, PlanDescriptor Descriptor);

public sealed record PlanActivatedEvent(PlanId PlanId);

public sealed record PlanDeprecatedEvent(PlanId PlanId);
