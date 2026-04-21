using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public static class AuthorityErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("Authority ID must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Authority descriptor must have a non-empty cluster reference and authority name.");

    public static DomainException InvalidStateTransition(AuthorityStatus status, string action)
        => new DomainInvariantViolationException($"Cannot perform '{action}' when authority status is '{status}'.");

    public static DomainException InvalidParent()
        => new DomainInvariantViolationException("Authority parent cluster is not in an active state.");

    public static DomainException InvalidProviderBinding()
        => new DomainInvariantViolationException("Provider category is not permitted under this authority role.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Authority has already been initialized.");
}
