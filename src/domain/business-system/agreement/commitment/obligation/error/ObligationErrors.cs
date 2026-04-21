using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public static class ObligationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ObligationId is required and must not be empty.");

    public static DomainException AlreadyFulfilled(ObligationId id)
        => new DomainInvariantViolationException($"Obligation '{id.Value}' has already been fulfilled.");

    public static DomainException AlreadyBreached(ObligationId id)
        => new DomainInvariantViolationException($"Obligation '{id.Value}' has already been breached.");

    public static DomainException InvalidStateTransition(ObligationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Obligation has already been initialized.");
}
