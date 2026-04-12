namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public sealed record EligibilityCreatedEvent(EligibilityId EligibilityId, SubjectId SubjectId, string CriteriaDescription);
