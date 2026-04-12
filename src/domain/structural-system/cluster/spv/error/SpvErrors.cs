namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public static class SpvErrors
{
    public static InvalidOperationException MissingId()
        => new("SpvId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("SpvDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(SpvStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
