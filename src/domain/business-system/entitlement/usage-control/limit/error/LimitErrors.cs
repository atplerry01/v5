using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public static class LimitErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("LimitId is required and must not be empty.");

    public static DomainException MissingSubjectId()
        => new DomainInvariantViolationException("LimitSubjectId is required and must not be empty.");

    public static DomainException InvalidThreshold()
        => new DomainInvariantViolationException("ThresholdValue must be greater than zero.");

    public static DomainException InvalidStateTransition(LimitStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyEnforced(LimitId id)
        => new DomainInvariantViolationException($"Limit '{id.Value}' has already been enforced.");

    public static DomainException AlreadyBreached(LimitId id)
        => new DomainInvariantViolationException($"Limit '{id.Value}' has already been breached.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Limit has already been initialized.");
}
