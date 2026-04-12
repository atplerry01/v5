namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public static class TypeDefinitionErrors
{
    public static InvalidOperationException MissingId()
        => new("TypeDefinitionId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("TypeDefinitionDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(TypeDefinitionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
