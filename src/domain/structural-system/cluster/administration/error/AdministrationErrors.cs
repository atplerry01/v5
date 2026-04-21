using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public static class AdministrationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("Administration ID must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Administration descriptor must have a non-empty cluster reference and administration name.");

    public static DomainException InvalidStateTransition(AdministrationStatus status, string action)
        => new DomainInvariantViolationException($"Cannot perform '{action}' when administration status is '{status}'.");

    public static DomainException InvalidParent()
        => new DomainInvariantViolationException("Administration parent cluster is not in an active state.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Administration has already been initialized.");
}
