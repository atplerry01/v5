using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public static class RenewalErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("RenewalId is required and must not be empty.");

    public static DomainException MissingSourceId()
        => new DomainInvariantViolationException("RenewalSourceId is required and must not be empty.");

    public static DomainException AlreadyRenewed(RenewalId id)
        => new DomainInvariantViolationException($"Renewal '{id.Value}' has already been renewed.");

    public static DomainException AlreadyExpired(RenewalId id)
        => new DomainInvariantViolationException($"Renewal '{id.Value}' has already expired. Cannot renew an expired entity.");

    public static DomainException InvalidStateTransition(RenewalStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Renewal has already been initialized.");
}
