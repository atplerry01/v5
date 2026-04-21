using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public static class LifecycleErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("LifecycleId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("LifecycleDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(LifecycleStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Lifecycle has already been initialized.");
}
