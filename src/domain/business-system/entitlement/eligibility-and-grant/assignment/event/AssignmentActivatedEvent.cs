namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentActivatedEvent(AssignmentId AssignmentId, DateTimeOffset ActivatedAt);
