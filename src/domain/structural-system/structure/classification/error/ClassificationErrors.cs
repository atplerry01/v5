namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public static class ClassificationErrors
{
    public static InvalidOperationException MissingId()
        => new("ClassificationId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("ClassificationDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(ClassificationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
