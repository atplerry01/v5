using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public static class HierarchyDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("HierarchyDefinitionId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("HierarchyDefinitionDescriptor is required and must not be default.");

    public static DomainException InvalidParentChild()
        => new DomainInvariantViolationException("A hierarchy definition cannot reference itself as its own parent.");

    public static DomainException InvalidStateTransition(HierarchyDefinitionStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("HierarchyDefinition has already been initialized.");
}
