namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed record RestrictionCreatedEvent(RestrictionId RestrictionId, RestrictionSubjectId SubjectId, string ConditionDescription);
