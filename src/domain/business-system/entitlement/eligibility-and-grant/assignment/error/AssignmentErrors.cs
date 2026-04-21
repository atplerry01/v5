using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public static class AssignmentErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AssignmentId is required and must not be empty.");

    public static DomainException MissingGrantRef()
        => new DomainInvariantViolationException("Assignment must reference a grant.");

    public static DomainException MissingSubject()
        => new DomainInvariantViolationException("Assignment requires a subject ref.");

    public static DomainException InvalidStateTransition(AssignmentStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyRevoked(AssignmentId id)
        => new DomainInvariantViolationException($"Assignment '{id.Value}' is already revoked.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Assignment has already been initialized.");
}
