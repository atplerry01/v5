using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class ClusterLifecycleAggregate : AggregateRoot
{
    private static readonly LifecycleTransitionSpec TransitionSpec = new();

    public ClusterId ClusterId { get; private set; } = default!;
    public LifecycleStatus Status { get; private set; } = LifecycleStatus.Initializing;

    private ClusterLifecycleAggregate() { }

    public static ClusterLifecycleAggregate Initialize(Guid lifecycleId, ClusterId clusterId)
    {
        Guard.AgainstDefault(lifecycleId);
        Guard.AgainstNull(clusterId);

        var lifecycle = new ClusterLifecycleAggregate
        {
            Id = lifecycleId,
            ClusterId = clusterId,
            Status = LifecycleStatus.Initializing
        };

        return lifecycle;
    }

    public void Activate(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status != LifecycleStatus.Archived,
            "LIFECYCLE_TRANSITION",
            LifecycleErrors.CannotActivateFromArchived);

        EnsureInvariant(
            Status != LifecycleStatus.Active,
            "ALREADY_IN_STATE",
            LifecycleErrors.AlreadyActive);

        EnsureInvariant(
            TransitionSpec.CanActivate(Status),
            "INVALID_STATE_TRANSITION",
            $"Cannot activate from '{Status}' status.");

        Status = LifecycleStatus.Active;

        RaiseDomainEvent(new ClusterLifecycleActivatedEvent(
            ClusterId.Value,
            timestamp));
    }

    public void Suspend(string reason)
    {
        Guard.AgainstEmpty(reason);

        EnsureInvariant(
            Status != LifecycleStatus.Suspended,
            "ALREADY_IN_STATE",
            LifecycleErrors.AlreadySuspended);

        EnsureInvariant(
            TransitionSpec.CanSuspend(Status),
            "INVALID_STATE_TRANSITION",
            LifecycleErrors.CannotSuspendUnlessActive);

        Status = LifecycleStatus.Suspended;

        RaiseDomainEvent(new ClusterSuspendedEvent(
            ClusterId.Value,
            reason));
    }

    public void Archive(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status != LifecycleStatus.Archived,
            "ALREADY_IN_STATE",
            LifecycleErrors.AlreadyArchived);

        EnsureInvariant(
            TransitionSpec.CanArchive(Status),
            "INVALID_STATE_TRANSITION",
            LifecycleErrors.CannotArchiveFromInitializing);

        Status = LifecycleStatus.Archived;

        RaiseDomainEvent(new ClusterArchivedEvent(
            ClusterId.Value,
            timestamp));
    }
}
