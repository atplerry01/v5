using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public static class SubclusterErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("SubclusterId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("SubclusterDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(SubclusterStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException InvalidParent()
        => new DomainInvariantViolationException("Subcluster parent cluster is not in an active state.");

    public static DomainException InvalidAuthorityReporting()
        => new DomainInvariantViolationException("Authority role is not permitted to govern this subcluster.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Subcluster has already been initialized.");
}
