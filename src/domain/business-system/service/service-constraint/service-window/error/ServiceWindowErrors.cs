using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public static class ServiceWindowErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ServiceWindowId is required and must not be empty.");

    public static DomainException MissingServiceDefinitionRef()
        => new DomainInvariantViolationException("ServiceWindow must reference a service definition.");

    public static DomainException InvalidStateTransition(ServiceWindowStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ServiceWindowId id)
        => new DomainInvariantViolationException($"ServiceWindow '{id.Value}' is archived and cannot be mutated.");

    public static DomainException ClosedWindowRequired()
        => new DomainInvariantViolationException("ServiceWindow range requires a closed TimeWindow (EndsAt must be set).");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ServiceWindow has already been initialized.");
}
