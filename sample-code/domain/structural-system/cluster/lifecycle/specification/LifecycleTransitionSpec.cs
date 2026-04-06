namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class LifecycleTransitionSpec
{
    public bool CanActivate(LifecycleStatus current) =>
        current is LifecycleStatus.Initializing or LifecycleStatus.Suspended;

    public bool CanSuspend(LifecycleStatus current) =>
        current is LifecycleStatus.Active;

    public bool CanArchive(LifecycleStatus current) =>
        current is LifecycleStatus.Active or LifecycleStatus.Suspended;
}
