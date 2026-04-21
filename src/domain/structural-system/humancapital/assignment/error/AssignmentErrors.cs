using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Assignment;

public static class AssignmentErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Assignment has already been initialized.");

    public static DomainException InactiveParent(StructuralParentState state)
        => new DomainInvariantViolationException(
            $"Cannot assign under ClusterAuthority with state '{state}'. Active parent required.");
}
