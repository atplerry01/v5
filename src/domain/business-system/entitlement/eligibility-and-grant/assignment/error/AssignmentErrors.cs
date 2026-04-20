namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public static class AssignmentErrors
{
    public static AssignmentDomainException MissingId()
        => new("AssignmentId is required and must not be empty.");

    public static AssignmentDomainException MissingGrantRef()
        => new("Assignment must reference a grant.");

    public static AssignmentDomainException MissingSubject()
        => new("Assignment requires a subject ref.");

    public static AssignmentDomainException InvalidStateTransition(AssignmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AssignmentDomainException AlreadyRevoked(AssignmentId id)
        => new($"Assignment '{id.Value}' is already revoked.");
}

public sealed class AssignmentDomainException : Exception
{
    public AssignmentDomainException(string message) : base(message) { }
}
