using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public static class AllocationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AllocationId is required and must not be empty.");

    public static DomainException MissingResourceId()
        => new DomainInvariantViolationException("ResourceId is required and must not be empty.");

    public static DomainException InvalidStateTransition(AllocationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException CapacityExceeded(int requested, int available)
        => new DomainInvariantViolationException($"Cannot allocate {requested} units. Only {available} available.");

    public static DomainException AlreadyAllocated(AllocationId id)
        => new DomainInvariantViolationException($"Allocation '{id.Value}' has already been allocated.");

    public static DomainException AlreadyReleased(AllocationId id)
        => new DomainInvariantViolationException($"Allocation '{id.Value}' has already been released.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Allocation has already been initialized.");
}
