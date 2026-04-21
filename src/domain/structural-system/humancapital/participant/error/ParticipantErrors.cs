using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public static class ParticipantErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Participant has already been initialized.");

    public static DomainException InactiveParent(StructuralParentState state)
        => new DomainInvariantViolationException(
            $"Cannot place participant under Cluster with state '{state}'. Active parent required.");
}
