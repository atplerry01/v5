using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public static class ContactPointErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ContactPointId is required and must not be empty.");

    public static DomainException MissingCustomerRef()
        => new DomainInvariantViolationException("ContactPoint must reference a customer.");

    public static DomainException InvalidStateTransition(ContactPointStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ContactPointId id)
        => new DomainInvariantViolationException($"ContactPoint '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ContactPoint has already been initialized.");
}
