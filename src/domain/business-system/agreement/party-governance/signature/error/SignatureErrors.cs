using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public static class SignatureErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("SignatureId is required and must not be empty.");

    public static DomainException AlreadySigned(SignatureId id)
        => new DomainInvariantViolationException($"Signature '{id.Value}' has already been signed.");

    public static DomainException AlreadyRevoked(SignatureId id)
        => new DomainInvariantViolationException($"Signature '{id.Value}' has already been revoked.");

    public static DomainException InvalidStateTransition(SignatureStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Signature has already been initialized.");
}
