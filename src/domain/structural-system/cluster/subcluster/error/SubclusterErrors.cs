namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public static class SubclusterErrors
{
    public static InvalidOperationException MissingId()
        => new("SubclusterId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("SubclusterDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(SubclusterStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
