using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public static class EligibilityErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("EligibilityId is required and must not be empty.");

    public static DomainException MissingSubject()
        => new DomainInvariantViolationException("Eligibility requires a subject ref.");

    public static DomainException MissingTarget()
        => new DomainInvariantViolationException("Eligibility requires a target ref.");

    public static DomainException InvalidStateTransition(EligibilityStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyEvaluated(EligibilityId id, EligibilityStatus status)
        => new DomainInvariantViolationException($"Eligibility '{id.Value}' is already evaluated ({status}).");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Eligibility has already been initialized.");
}
