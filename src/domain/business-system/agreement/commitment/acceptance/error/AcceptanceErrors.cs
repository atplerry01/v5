using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public static class AcceptanceErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AcceptanceId is required and must not be empty.");

    public static DomainException AlreadyAccepted(AcceptanceId id)
        => new DomainInvariantViolationException($"Acceptance '{id.Value}' has already been accepted.");

    public static DomainException AlreadyRejected(AcceptanceId id)
        => new DomainInvariantViolationException($"Acceptance '{id.Value}' has already been rejected.");

    public static DomainException InvalidStateTransition(AcceptanceStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Acceptance has already been initialized.");
}
