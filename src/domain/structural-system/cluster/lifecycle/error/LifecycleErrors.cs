namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public static class LifecycleErrors
{
    public static InvalidOperationException MissingId()
        => new("LifecycleId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("LifecycleDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(LifecycleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
