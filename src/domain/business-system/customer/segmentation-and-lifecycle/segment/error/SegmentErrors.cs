using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public static class SegmentErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("SegmentId is required and must not be empty.");

    public static DomainException InvalidStateTransition(SegmentStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(SegmentId id)
        => new DomainInvariantViolationException($"Segment '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Segment has already been initialized.");
}
