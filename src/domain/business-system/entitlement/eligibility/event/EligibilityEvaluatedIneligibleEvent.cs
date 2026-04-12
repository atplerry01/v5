namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public sealed record EligibilityEvaluatedIneligibleEvent(EligibilityId EligibilityId, string Reason);
