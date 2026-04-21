using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public static class SpvErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("SpvId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("SpvDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(SpvStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException InvalidParent()
        => new DomainInvariantViolationException("SPV parent cluster is not in an active state.");

    public static DomainException InvalidScope()
        => new DomainInvariantViolationException("SPV type is not permitted under the current subcluster scope.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("SPV has already been initialized.");
}
