namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public static class LifecycleErrors
{
    public static string AlreadyActive => "Cluster is already active.";
    public static string AlreadySuspended => "Cluster is already suspended.";
    public static string AlreadyArchived => "Cluster is already archived.";
    public static string CannotActivateFromArchived => "Cannot activate an archived cluster.";
    public static string CannotSuspendUnlessActive => "Only active clusters can be suspended.";
    public static string CannotArchiveFromInitializing => "Cannot archive a cluster that is still initializing.";
}
