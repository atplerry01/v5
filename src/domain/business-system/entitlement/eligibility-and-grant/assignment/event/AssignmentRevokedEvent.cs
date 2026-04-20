namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentRevokedEvent(AssignmentId AssignmentId, DateTimeOffset RevokedAt);
