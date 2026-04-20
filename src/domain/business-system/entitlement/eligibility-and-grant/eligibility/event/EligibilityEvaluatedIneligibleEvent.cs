namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityEvaluatedIneligibleEvent(
    EligibilityId EligibilityId,
    IneligibilityReason Reason,
    DateTimeOffset EvaluatedAt);
