using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public static class ValidityErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ValidityId is required and must not be empty.");

    public static DomainException AlreadyInvalid(ValidityId id)
        => new DomainInvariantViolationException($"Validity '{id.Value}' is already invalid.");

    public static DomainException AlreadyExpired(ValidityId id)
        => new DomainInvariantViolationException($"Validity '{id.Value}' has already expired.");

    public static DomainException InvalidStateTransition(ValidityStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Validity has already been initialized.");
}
