namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityCreatedEvent(
    EligibilityId EligibilityId,
    EligibilitySubjectRef Subject,
    EligibilityTargetRef Target,
    EligibilityScope Scope);
