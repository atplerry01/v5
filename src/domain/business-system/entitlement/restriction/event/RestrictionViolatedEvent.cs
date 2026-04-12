namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed record RestrictionViolatedEvent(RestrictionId RestrictionId, string ViolationReason);
