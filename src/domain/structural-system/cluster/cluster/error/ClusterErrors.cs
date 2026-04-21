using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public static class ClusterErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ClusterId cannot be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("ClusterDescriptor requires non-empty ClusterName and ClusterType.");

    public static DomainException InvalidStateTransition(ClusterStatus status, string action)
        => new DomainInvariantViolationException($"Cannot {action} a cluster in {status} status.");

    public static DomainException DuplicateAuthority()
        => new DomainInvariantViolationException("Authority is already bound to this cluster.");

    public static DomainException DuplicateAdministration()
        => new DomainInvariantViolationException("Administration is already bound to this cluster.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Cluster has already been initialized.");
}
