namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentCreatedEvent(
    AssignmentId AssignmentId,
    GrantRef Grant,
    AssignmentSubjectRef Subject,
    AssignmentScope Scope);
