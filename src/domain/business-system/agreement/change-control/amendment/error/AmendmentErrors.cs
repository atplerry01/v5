using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public static class AmendmentErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AmendmentId is required and must not be empty.");

    public static DomainException MissingTargetId()
        => new DomainInvariantViolationException("AmendmentTargetId is required and must not be empty.");

    public static DomainException AlreadyApplied(AmendmentId id)
        => new DomainInvariantViolationException($"Amendment '{id.Value}' has already been applied.");

    public static DomainException AlreadyReverted(AmendmentId id)
        => new DomainInvariantViolationException($"Amendment '{id.Value}' has already been reverted.");

    public static DomainException InvalidStateTransition(AmendmentStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Amendment has already been initialized.");
}
