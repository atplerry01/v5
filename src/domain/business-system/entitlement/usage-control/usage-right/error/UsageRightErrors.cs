using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public static class UsageRightErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("UsageRightId is required and must not be empty.");

    public static DomainException MissingSubjectId()
        => new DomainInvariantViolationException("UsageRightSubjectId is required and must not be empty.");

    public static DomainException MissingReferenceId()
        => new DomainInvariantViolationException("UsageRightReferenceId is required and must not be empty.");

    public static DomainException InvalidTotalUnits()
        => new DomainInvariantViolationException("TotalUnits must be greater than zero.");

    public static DomainException InvalidStateTransition(UsageRightStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException UsageExceedsAvailable(int requested, int remaining)
        => new DomainInvariantViolationException($"Cannot use {requested} units. Only {remaining} remaining.");

    public static DomainException UsageRemaining(int remaining)
        => new DomainInvariantViolationException($"Cannot consume usage right with {remaining} units remaining.");

    public static DomainException AlreadyConsumed(UsageRightId id)
        => new DomainInvariantViolationException($"UsageRight '{id.Value}' has already been fully consumed.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("UsageRight has already been initialized.");
}
