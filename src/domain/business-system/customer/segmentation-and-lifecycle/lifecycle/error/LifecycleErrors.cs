using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public static class LifecycleErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("LifecycleId is required and must not be empty.");

    public static DomainException MissingCustomerRef()
        => new DomainInvariantViolationException("Lifecycle must reference a customer.");

    public static DomainException InvalidStageTransition(LifecycleStage from, LifecycleStage to)
        => new DomainInvariantViolationException($"Cannot transition lifecycle from '{from}' to '{to}'.");

    public static DomainException ClosedImmutable(LifecycleId id)
        => new DomainInvariantViolationException($"Lifecycle '{id.Value}' is closed and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Lifecycle has already been initialized.");
}
