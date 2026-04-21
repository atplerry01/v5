using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public static class GrantErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("GrantId is required and must not be empty.");

    public static DomainException MissingSubject()
        => new DomainInvariantViolationException("Grant requires a subject ref.");

    public static DomainException MissingTarget()
        => new DomainInvariantViolationException("Grant requires a target ref.");

    public static DomainException InvalidStateTransition(GrantStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyTerminal(GrantId id, GrantStatus status)
        => new DomainInvariantViolationException($"Grant '{id.Value}' is already terminal ({status}) and cannot be reactivated.");

    public static DomainException ExpiryInPast()
        => new DomainInvariantViolationException("GrantExpiry cannot already be in the past at the moment of creation or activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Grant has already been initialized.");
}
