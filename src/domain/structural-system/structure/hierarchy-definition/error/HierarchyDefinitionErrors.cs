namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public static class HierarchyDefinitionErrors
{
    public static InvalidOperationException MissingId()
        => new("HierarchyDefinitionId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("HierarchyDefinitionDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidParentChild()
        => new("A hierarchy definition cannot reference itself as its own parent.");

    public static InvalidOperationException InvalidStateTransition(HierarchyDefinitionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
