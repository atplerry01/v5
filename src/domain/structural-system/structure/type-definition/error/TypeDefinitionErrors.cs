using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public static class TypeDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("TypeDefinitionId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("TypeDefinitionDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(TypeDefinitionStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("TypeDefinition has already been initialized.");
}
